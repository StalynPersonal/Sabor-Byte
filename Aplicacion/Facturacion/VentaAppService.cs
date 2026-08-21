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
    INotificadorComandas notificadorComandas)
{
    public async Task<VentaResultadoDto> CrearVentaAsync(
        Guid sucursalId, Guid usuarioId, CrearVentaRequestDto request, CancellationToken ct = default)
    {
        // Facturar desde una comanda existente (mesero -> cocina -> caja): los items se
        // toman de la comanda, no del request — evita que caja invente/altere lo que se
        // preparó en cocina. El inventario ya se descontó al enviar la comanda a cocina.
        Comanda? comandaOrigen = null;
        if (request.ComandaId is not null)
        {
            if (request.Items.Count > 0)
                throw new InvalidOperationException("Al facturar desde una comanda, Items debe venir vacío.");

            comandaOrigen = await db.Comandas
                .Include(c => c.Items).ThenInclude(i => i.IngredientesExcluidos)
                .FirstOrDefaultAsync(c => c.Id == request.ComandaId && c.SucursalId == sucursalId, ct)
                ?? throw new InvalidOperationException("La comanda no existe.");

            if (comandaOrigen.Estado is EstadoComanda.Cerrada or EstadoComanda.Cancelada)
                throw new InvalidOperationException($"No se puede facturar una comanda en estado '{comandaOrigen.Estado}'.");

            var itemsVigentes = comandaOrigen.Items.Where(i => i.Estado != EstadoItemComanda.Cancelado).ToList();
            if (itemsVigentes.Count == 0)
                throw new InvalidOperationException("La comanda no tiene ítems vigentes para facturar.");

            request.Items = itemsVigentes.Select(i => new ItemVentaDto
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                IngredientesExcluidosIds = i.IngredientesExcluidos.Select(e => e.IngredienteId).ToList()
            }).ToList();
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

        var productoIds = request.Items.Select(i => i.ProductoId).ToList();
        var productos = await db.Productos
            .Where(p => productoIds.Contains(p.Id) && p.SucursalId == sucursalId)
            .ToDictionaryAsync(p => p.Id, ct);

        var factura = new Factura
        {
            SucursalId = sucursalId,
            CajaTurnoId = turno.Id,
            ClienteId = request.ClienteId,
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
            // Tasa por producto, no fija (sección 9 del plan): AplicaItbis=false = exento
            // (sin tasa); AplicaItbis=true usa Producto.TasaItbis (18%/16%/0%).
            var tasaAplicada = producto.AplicaItbis ? producto.TasaItbis : (decimal?)null;
            var itbisLinea = tasaAplicada is decimal tasa ? Math.Round(totalLinea * tasa, 2) : 0m;

            subtotal += producto.Precio * item.Cantidad;
            descuentoTotal += item.Descuento;
            itbis += itbisLinea;

            factura.Detalle.Add(new FacturaDetalle
            {
                FacturaId = factura.Id,
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                Descuento = item.Descuento,
                TasaItbis = tasaAplicada,
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

        await AsignarNcfSiAplicaAsync(sucursalId, factura, ct);

        db.Facturas.Add(factura);

        db.MovimientosCaja.Add(new MovimientoCaja
        {
            TurnoCajaId = turno.Id,
            Tipo = TipoMovimientoCaja.Venta,
            FacturaId = factura.Id,
            FormaPago = request.FormaPago,
            Monto = factura.Total,
            Descripcion = $"Venta {factura.NumeroNcf ?? factura.Id.ToString()[..8]}"
        });

        // Si viene de una comanda, el inventario ya se descontó al enviarla a cocina
        // (ComandaAppService.CrearComandaAsync) — descontarlo de nuevo aquí duplicaría el consumo.
        if (comandaOrigen is null)
        {
            foreach (var item in request.Items)
            {
                await inventario.DescontarPorVentaAsync(
                    sucursalId, item.ProductoId, item.Cantidad, factura.Id,
                    item.IngredientesExcluidosIds, usuarioId, ct);
            }
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

        return new VentaResultadoDto
        {
            FacturaId = factura.Id,
            NumeroNcf = factura.NumeroNcf,
            Subtotal = factura.Subtotal,
            Itbis = factura.Itbis,
            Descuento = factura.Descuento,
            Propina = factura.Propina,
            Total = factura.Total,
            FechaEmision = factura.FechaEmision
        };
    }

    // v1: sin e-CF (Fase 4 pendiente). Si hay una secuencia NCF tradicional activa, se asigna;
    // si no, la factura queda "sin NCF" (consumo interno), sin bloquear la venta.
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
    private async Task AsignarNcfSiAplicaAsync(Guid sucursalId, Factura factura, CancellationToken ct)
    {
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
            factura.EstadoDgii = EstadoDgii.NoAplica; // no es e-CF, es NCF tradicional
            return;
        }
    }
}
