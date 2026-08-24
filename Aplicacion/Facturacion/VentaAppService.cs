using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Facturacion.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Pedidos.Dtos;
using SaborByte.Dominio.Caja;
using SaborByte.Dominio.Facturacion;
using SaborByte.Dominio.Pedidos;

namespace SaborByte.Aplicacion.Facturacion;

public class VentaAppService(
    IAppDbContext db,
    Inventario.InventarioAppService inventario,
    Identidad.AutorizacionAppService autorizacion,
    IAuditoriaService auditoria,
    INotificadorComandas notificadorComandas,
    IFacturacionElectronicaGateway facturacionElectronica)
{
    public async Task<VentaResultadoDto> CrearVentaAsync(
        Guid sucursalId, Guid usuarioId, CrearVentaRequestDto request, CancellationToken ct = default)
    {
        // Facturar desde una comanda existente (mesero -> cocina -> caja): los items de la
        // comanda se toman de la comanda, no del request, para que caja no pueda inventar/
        // alterar lo que se preparó en cocina — pero sí puede AGREGAR productos extra que no
        // estaban en la comanda (ej. algo que el cliente pidió al momento de cobrar), que
        // llegan en request.Items y sí se descuentan de inventario abajo (itemsDesdeComanda
        // los distingue: esos ya se descontaron al enviar la comanda a cocina).
        Comanda? comandaOrigen = null;
        var itemsDesdeComanda = new HashSet<ItemVentaDto>();
        if (request.ComandaId is not null)
        {
            var itemsExtra = request.Items;

            comandaOrigen = await db.Comandas
                .Include(c => c.Items).ThenInclude(i => i.IngredientesExcluidos)
                .FirstOrDefaultAsync(c => c.Id == request.ComandaId && c.SucursalId == sucursalId, ct)
                ?? throw new InvalidOperationException("La comanda no existe.");

            if (comandaOrigen.Estado is EstadoComanda.Cerrada or EstadoComanda.Cancelada)
                throw new InvalidOperationException($"No se puede facturar una comanda en estado '{comandaOrigen.Estado}'.");

            var itemsVigentes = comandaOrigen.Items.Where(i => i.Estado != EstadoItemComanda.Cancelado).ToList();
            if (itemsVigentes.Count == 0 && itemsExtra.Count == 0)
                throw new InvalidOperationException("La comanda no tiene ítems vigentes para facturar.");

            var itemsComanda = itemsVigentes.Select(i => new ItemVentaDto
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                IngredientesExcluidosIds = i.IngredientesExcluidos.Select(e => e.IngredienteId).ToList()
            }).ToList();

            itemsDesdeComanda = [.. itemsComanda];
            request.Items = [.. itemsComanda, .. itemsExtra];
        }

        if (request.Items.Count == 0)
            throw new InvalidOperationException("La venta debe tener al menos un producto.");

        // Descuentos solo con autorización de Supervisor/Admin (sección 7 del plan).
        if (request.Items.Any(i => i.Descuento > 0))
        {
            if (request.CodigoAutorizacionDescuento is null)
                throw new InvalidOperationException("El descuento requiere autorización de un Supervisor o Administrador.");

            await autorizacion.ValidarYConsumirAsync(request.CodigoAutorizacionDescuento.Value, "Descuento", ct);
            await auditoria.RegistrarAsync(sucursalId, usuarioId, "Descuento", "Factura",
                detalle: $"Monto descuento: {request.Items.Sum(i => i.Descuento):0.00}", ct: ct);
        }

        // Evita IDOR: sin el join con Cajas, un usuario con acceso a su propia sucursal
        // podía facturar contra el turno de OTRA sucursal con solo conocer su GUID.
        var turno = await (
            from t in db.TurnosCaja
            join c in db.Cajas on t.CajaId equals c.Id
            where t.Id == request.TurnoCajaId && c.SucursalId == sucursalId
            select t
        ).FirstOrDefaultAsync(ct) ?? throw new InvalidOperationException("El turno de caja no existe.");

        if (turno.Estado != EstadoTurnoCaja.Abierto)
            throw new InvalidOperationException("No se puede facturar sobre un turno de caja cerrado.");

        // Un turno no puede cruzar de un día calendario a otro sin cerrarse (mismo
        // criterio en UTC que CajaAppService.ObtenerTurnoAbiertoAsync).
        if (turno.FechaHoraApertura.Date != DateTime.UtcNow.Date)
            throw new InvalidOperationException(
                $"Este turno se abrió el {turno.FechaHoraApertura:dd/MM/yyyy} y no puede seguir operando en otro día. Debe cerrarlo antes de continuar.");

        var productoIds = request.Items.Select(i => i.ProductoId).ToList();
        var productos = await db.Productos
            .Include(p => p.UnidadMedida)
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        // El cliente ahora es obligatorio para facturar (antes se caía en "Cliente Contado"
        // en silencio si no se elegía ninguno) — el cajero debe elegir uno explícitamente,
        // aunque sea el genérico "Cliente Contado" para un walk-in sin datos.
        if (request.ClienteId is not Guid clienteId || clienteId == Guid.Empty)
            throw new InvalidOperationException("Debes seleccionar un cliente para facturar.");

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El cliente no existe.");

        var caja = await db.Cajas.AsNoTracking()
            .Where(c => c.Id == turno.CajaId)
            .Select(c => new { c.Numero, c.CodigoSucursal })
            .FirstAsync(ct);

        var sucursal = await db.Sucursales.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId, ct)
            ?? throw new InvalidOperationException("La sucursal no existe.");

        var factura = new Factura
        {
            SucursalId = sucursalId,
            CajaTurnoId = turno.Id,
            ClienteId = cliente.Id,
            ClienteNombre = cliente.NombreORazonSocial,
            ClienteRncOCedula = cliente.RncOCedula,
            SucursalCodigo = caja.CodigoSucursal,
            CajaCodigo = caja.Numero,
            ComandaId = comandaOrigen?.Id,
            CreadoPorUsuarioId = usuarioId,
            FechaEmision = DateTime.UtcNow
        };

        decimal subtotal = 0, itbis = 0, descuentoTotal = 0;

        foreach (var item in request.Items)
        {
            if (!productos.TryGetValue(item.ProductoId, out var producto))
                throw new InvalidOperationException($"El producto {item.ProductoId} no existe.");

            var totalLinea = (producto.Precio * item.Cantidad) - item.Descuento;
            // Tasa por producto, no fija (sección 9 del plan): todo producto lleva
            // ITBIS a la tasa que tenga configurada (18%/16%/0% — no existe "exento").
            var itbisLinea = Math.Round(totalLinea * producto.TasaItbis, 2);

            subtotal += producto.Precio * item.Cantidad;
            descuentoTotal += item.Descuento;
            itbis += itbisLinea;

            factura.Detalle.Add(new FacturaDetalle
            {
                FacturaId = factura.Id,
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                Codigo = producto.Codigo,
                UnidadMedida = producto.UnidadMedida?.Nombre ?? "Unidad",
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                Descuento = item.Descuento,
                TasaItbis = producto.TasaItbis,
                Itbis = itbisLinea,
                Total = totalLinea + itbisLinea
            });
        }

        var propina = request.MontoPropinaFijo ?? (request.PorcentajePropina is > 0
            ? Math.Round((subtotal - descuentoTotal + itbis) * (request.PorcentajePropina.Value / 100m), 2)
            : 0m);

        factura.Subtotal = subtotal;
        factura.Descuento = descuentoTotal;
        factura.Itbis = itbis;
        factura.Propina = propina;
        factura.Total = subtotal - descuentoTotal + itbis + propina;

        // Una factura puede pagarse con más de una forma de pago (ej. mitad efectivo,
        // mitad tarjeta) — se guarda el desglose en FacturaPago y, además, un
        // MovimientoCaja por cada una para que el cuadre de turno (agrupado por
        // MetodoPagoId) siga funcionando igual que con un solo pago.
        var metodoPagoIds = request.Pagos.Select(p => p.MetodoPagoId).Distinct().ToList();
        var metodosPago = await db.MetodosPago
            .Where(m => metodoPagoIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        if (request.Pagos.Any(p => !metodosPago.ContainsKey(p.MetodoPagoId)))
            throw new InvalidOperationException("Una de las formas de pago seleccionadas no existe.");

        var cambio = ValidarPagosYCalcularCambio(request.Pagos, factura.Total, metodosPago);

        await AsignarNumeroFacturaAsync(sucursal, turno.CajaId, factura, ct);
        await AsignarNcfSiAplicaAsync(sucursalId, sucursal.EcfActivo, factura, ct);

        db.Facturas.Add(factura);

        // Solo puede haber cambio cuando es un único pago en efectivo (ver
        // ValidarPagosYCalcularCambio) — en ese caso, lo que se registra como venta/ingreso
        // es el total, no el efectivo recibido completo; el resto es cambio que sale de la
        // caja, no ingreso. Para cualquier otro caso (varias formas de pago, o una sola que
        // no es efectivo) el monto ya viene validado exacto y se registra tal cual.
        var pagosParaResultado = new List<PagoVentaRequestDto>();
        foreach (var pago in request.Pagos)
        {
            var metodo = metodosPago[pago.MetodoPagoId];
            var montoAplicado = cambio > 0 ? pago.Monto - cambio : pago.Monto;

            factura.Pagos.Add(new FacturaPago
            {
                FacturaId = factura.Id,
                MetodoPagoId = pago.MetodoPagoId,
                Monto = montoAplicado,
                NumeroComprobante = metodo.RequiereComprobante ? pago.NumeroComprobante : null
            });

            db.MovimientosCaja.Add(new MovimientoCaja
            {
                TurnoCajaId = turno.Id,
                Tipo = TipoMovimientoCaja.Venta,
                FacturaId = factura.Id,
                MetodoPagoId = pago.MetodoPagoId,
                Monto = montoAplicado,
                Descripcion = $"Venta {factura.NumeroFactura}"
            });

            pagosParaResultado.Add(new PagoVentaRequestDto
            {
                MetodoPagoId = pago.MetodoPagoId,
                Monto = montoAplicado,
                NumeroComprobante = pago.NumeroComprobante
            });
        }

        // Los ítems que vienen de la comanda ya descontaron inventario al enviarla a cocina
        // (ComandaAppService.CrearComandaAsync) — descontarlo de nuevo aquí duplicaría el
        // consumo. Los ítems extra agregados al momento de cobrar sí se descuentan aquí.
        foreach (var item in request.Items)
        {
            if (itemsDesdeComanda.Contains(item))
                continue;

            await inventario.DescontarPorVentaAsync(
                sucursalId, item.ProductoId, item.Cantidad, factura.Id,
                item.IngredientesExcluidosIds, usuarioId, ct);
        }

        if (comandaOrigen is not null)
        {
            comandaOrigen.Estado = EstadoComanda.Cerrada;

            if (comandaOrigen.MesaId is not null)
            {
                var mesa = await db.Mesas.FirstOrDefaultAsync(m => m.Id == comandaOrigen.MesaId, ct);
                if (mesa is not null)
                    mesa.Estado = EstadoMesa.Libre;
            }
        }

        await db.SaveChangesAsync(ct);

        if (comandaOrigen is not null)
            await notificadorComandas.ComandaCerradaAsync(sucursalId, comandaOrigen.Id);

        // e-CF real: solo si la sucursal lo activó y quedó un eNCF asignado arriba (serie "E"
        // en SecuenciaNcf). El gateway valida, firma y envía a DGII — nunca bloquea la venta:
        // si falla (validación, sin certificado, sin conexión), la factura ya quedó guardada
        // y con su ticket impreso; el estado de DGII se corrige después (reintento/consulta).
        string? mensajeDgii = null;
        if (sucursal.EcfActivo && factura.NumeroNcf is not null)
        {
            var resultadoEcf = await facturacionElectronica.EmitirAsync(factura.Id, ct);
            mensajeDgii = resultadoEcf.Exitoso
                ? resultadoEcf.MensajeDgii
                : $"e-CF no enviado: {string.Join("; ", resultadoEcf.ErroresValidacion)}";
        }

        var cajeroNombre = await db.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Nombre).FirstOrDefaultAsync(ct);

        return new VentaResultadoDto
        {
            FacturaId = factura.Id,
            NumeroFactura = factura.NumeroFactura,
            NumeroNcf = factura.NumeroNcf,
            Subtotal = factura.Subtotal,
            Itbis = factura.Itbis,
            Descuento = factura.Descuento,
            Propina = factura.Propina,
            Total = factura.Total,
            FechaEmision = factura.FechaEmision,
            Pagos = pagosParaResultado,
            Cambio = cambio,
            ClienteNombre = factura.ClienteNombre,
            ClienteRncOCedula = factura.ClienteRncOCedula,
            CajeroNombre = cajeroNombre,
            CodigoSeguridadDgii = factura.CodigoSeguridadDgii,
            MensajeDgii = mensajeDgii
        };
    }

    // Al menos un pago y montos positivos siempre. La suma debe cuadrar exacto con el
    // total, EXCEPTO cuando es un único pago en efectivo: ahí se permite recibir de más
    // (el cajero cobró con un billete grande) y se devuelve como cambio — nunca se permite
    // pagar de menos, ni dar cambio cuando hay varias formas de pago o una forma distinta
    // a efectivo (tarjeta/transferencia no dan "vuelto").
    private static decimal ValidarPagosYCalcularCambio(List<PagoVentaRequestDto> pagos, decimal total, Dictionary<Guid, Dominio.Catalogo.MetodoPago> metodosPago)
    {
        if (pagos.Count == 0)
            throw new InvalidOperationException("La venta debe indicar al menos una forma de pago.");

        if (pagos.Any(p => p.Monto <= 0))
            throw new InvalidOperationException("El monto de cada forma de pago debe ser mayor a cero.");

        var sumaPagos = pagos.Sum(p => p.Monto);
        var esPagoUnicoEnEfectivo = pagos.Count == 1 && metodosPago[pagos[0].MetodoPagoId].EsEfectivo;

        if (esPagoUnicoEnEfectivo)
        {
            if (sumaPagos < total - 0.01m)
                throw new InvalidOperationException(
                    $"El efectivo recibido (RD$ {sumaPagos:0.00}) es menor al total de la venta (RD$ {total:0.00}).");

            return Math.Max(0, Math.Round(sumaPagos - total, 2));
        }

        if (Math.Abs(sumaPagos - total) > 0.01m)
            throw new InvalidOperationException(
                $"La suma de las formas de pago (RD$ {sumaPagos:0.00}) no coincide con el total de la venta (RD$ {total:0.00}).");

        return 0m;
    }

    // Número interno de factura (no fiscal), siempre asignado: CodigoSucursal(2) + Caja.Numero(2)
    // + Secuencia(5) = 9 dígitos. Mismo patrón compare-and-swap que AsignarNcfSiAplicaAsync,
    // pero el contador vive en Caja.ProximoNumeroFactura porque la secuencia es por caja, no
    // por sucursal (dos cajas de la misma sucursal facturan en paralelo sin pisarse el número).
    private async Task AsignarNumeroFacturaAsync(Dominio.Sucursales.Sucursal sucursal, Guid cajaId, Factura factura, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sucursal.Codigo))
            throw new InvalidOperationException("La sucursal no tiene código configurado (2 dígitos); no se puede generar el número de factura.");

        while (true)
        {
            var caja = await db.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cajaId, ct)
                ?? throw new InvalidOperationException("La caja no existe.");

            var numeroReservado = caja.ProximoNumeroFactura;
            var filasActualizadas = await db.Cajas
                .Where(c => c.Id == cajaId && c.ProximoNumeroFactura == numeroReservado)
                .ExecuteUpdateAsync(c => c.SetProperty(x => x.ProximoNumeroFactura, x => x.ProximoNumeroFactura + 1), ct);

            if (filasActualizadas == 0)
                continue; // otra venta concurrente ya reservó este número; reintentar con el valor actualizado

            factura.NumeroFactura = $"{sucursal.Codigo}{caja.Numero}{numeroReservado:D5}";
            return;
        }
    }

    // Asigna el número (NCF tradicional o eNCF, según la serie de la secuencia activa) si hay
    // una secuencia vigente con cupo; si no, la factura queda "sin NCF" (consumo interno),
    // sin bloquear la venta. Cuando la sucursal tiene e-CF activo, el número asignado aquí es
    // el que CrearVentaAsync usa después para invocar al gateway (ver EstadoDgii.Pendiente
    // abajo); cuando no, es un NCF tradicional en papel y el flujo termina aquí mismo.
    //
    // La reserva del número es un compare-and-swap contra la base (ExecuteUpdateAsync, fuera
    // del change tracker), no una lectura + incremento en memoria persistido recién en el
    // SaveChanges final de CrearVentaAsync: con ese enfoque anterior, dos ventas concurrentes
    // podían leer el mismo SecuenciaProxima antes de que cualquiera escribiera, y ambas
    // terminaban con el mismo NCF — bug real encontrado con una prueba de carga de ventas
    // concurrentes (ver pruebas-carga/concurrencia-ventas.js). El UPDATE con el filtro exacto
    // "SecuenciaProxima == numeroReservado" hace que, bajo concurrencia, solo una de las
    // transacciones actualice la fila con ese valor; la otra recibe 0 filas y reintenta con
    // el valor ya avanzado.
    private async Task AsignarNcfSiAplicaAsync(Guid sucursalId, bool ecfActivo, Factura factura, CancellationToken ct)
    {
        // Si la sucursal no tiene activada "Facturación electrónica (e-CF/DGII)", nunca se
        // asigna NCF — aunque exista una secuencia configurada y vigente. El checkbox de la
        // sucursal es el interruptor único: sin él, la factura sale siempre "sin NCF".
        if (!ecfActivo)
        {
            factura.EstadoDgii = EstadoDgii.NoAplica;
            return;
        }

        while (true)
        {
            var secuencia = await db.SecuenciasNcf.AsNoTracking().FirstOrDefaultAsync(s =>
                s.SucursalId == sucursalId &&
                s.Activa &&
                s.FechaVencimiento > DateTime.UtcNow &&
                s.SecuenciaProxima <= s.SecuenciaFinal, ct);

            if (secuencia is null)
            {
                factura.EstadoDgii = EstadoDgii.NoAplica;
                return;
            }

            var numeroReservado = secuencia.SecuenciaProxima;
            var filasActualizadas = await db.SecuenciasNcf
                .Where(s => s.Id == secuencia.Id && s.SecuenciaProxima == numeroReservado)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.SecuenciaProxima, x => x.SecuenciaProxima + 1), ct);

            if (filasActualizadas == 0)
                continue; // otra venta concurrente ya reservó este número; reintentar con el valor actualizado

            factura.NumeroNcf = secuencia.FormatearNumero(numeroReservado);
            factura.TipoComprobante = secuencia.TipoComprobante;
            // Llegado aquí ecfActivo ya es true (si no, se salió arriba) — queda "Pendiente"
            // hasta que el gateway lo envíe a DGII más abajo en CrearVentaAsync, así si el
            // proceso se cae entre este punto y esa llamada, el estado refleja "en camino".
            factura.EstadoDgii = EstadoDgii.Pendiente;
            return;
        }
    }
}
