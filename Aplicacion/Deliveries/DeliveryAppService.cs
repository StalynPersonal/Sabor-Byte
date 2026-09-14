using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.Deliveries.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Comun;
using SaborByte.Dominio.Deliveries;

namespace SaborByte.Aplicacion.Deliveries;

public class DeliveryAppService(IAppDbContext db, IAuditoriaService auditoria)
{
    // Paginado, para el listado de administración en Central (ListarAsync de abajo se queda
    // sin paginar — lo usa la pestaña de Deliveries en Caja, que no tiene paginador en su UI).
    public async Task<ResultadoPaginado<DeliveryDto>> ListarPaginadoAsync(
        Guid sucursalId, bool incluirInactivos, string? texto, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 200);

        var query = db.Deliveries.Where(d => d.SucursalId == sucursalId && (incluirInactivos || d.Activo));

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(d => EF.Functions.Like(d.Nombre, $"%{texto}%"));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(d => d.Nombre)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(d => new DeliveryDto
            {
                Id = d.Id,
                Nombre = d.Nombre,
                Telefono = d.Telefono,
                Activo = d.Activo,
                SaldoPendiente = d.SaldoPendiente,
                LimiteSaldoPendiente = d.LimiteSaldoPendiente
            })
            .ToListAsync(ct);

        await CompletarAntiguedadAsync(items, ct);
        return new ResultadoPaginado<DeliveryDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = total };
    }

    public async Task<List<DeliveryDto>> ListarAsync(Guid sucursalId, bool incluirInactivos = false, string? texto = null, CancellationToken ct = default)
    {
        var query = db.Deliveries.Where(d => d.SucursalId == sucursalId && (incluirInactivos || d.Activo));

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(d => EF.Functions.Like(d.Nombre, $"%{texto}%"));

        var items = await query
            .OrderBy(d => d.Nombre)
            .Select(d => new DeliveryDto
            {
                Id = d.Id,
                Nombre = d.Nombre,
                Telefono = d.Telefono,
                Activo = d.Activo,
                SaldoPendiente = d.SaldoPendiente,
                LimiteSaldoPendiente = d.LimiteSaldoPendiente
            })
            .ToListAsync(ct);

        await CompletarAntiguedadAsync(items, ct);
        return items;
    }

    // La antigüedad requiere aplicar los abonos FIFO sobre las facturas de cada delivery
    // (ver CalcularDiasAntiguedadSaldoAsync) — no se puede expresar en una sola proyección
    // SQL, así que se completa acá, en memoria, después de traer la página/lista.
    private async Task CompletarAntiguedadAsync(List<DeliveryDto> items, CancellationToken ct)
    {
        foreach (var item in items)
        {
            if (item.SaldoPendiente > 0)
                item.DiasAntiguedadSaldo = await CalcularDiasAntiguedadSaldoAsync(item.Id, ct);
        }
    }

    // FIFO: se asume que los abonos cubren las facturas asignadas más antiguas primero
    // (el modelo no liga un abono a una factura específica, solo a la cuenta del
    // delivery en general) — la "antigüedad del saldo" es la fecha de la primera factura
    // que los abonos acumulados todavía no alcanzan a cubrir completa.
    private async Task<int?> CalcularDiasAntiguedadSaldoAsync(Guid deliveryId, CancellationToken ct)
    {
        var facturas = await db.FacturasDelivery
            .Where(fd => fd.DeliveryId == deliveryId && !fd.Quitada)
            .OrderBy(fd => fd.FechaAsignacion)
            .Select(fd => new { fd.FechaAsignacion, fd.MontoFactura })
            .ToListAsync(ct);

        var acumuladoAbonado = await db.AbonosDelivery
            .Where(a => a.DeliveryId == deliveryId && !a.Anulado)
            .SumAsync(a => (decimal?)a.Monto, ct) ?? 0;

        foreach (var factura in facturas)
        {
            if (acumuladoAbonado >= factura.MontoFactura)
            {
                acumuladoAbonado -= factura.MontoFactura;
                continue;
            }

            return (int)(DateTime.UtcNow - factura.FechaAsignacion).TotalDays;
        }

        return null;
    }

    public async Task<Guid> CrearAsync(Guid sucursalId, Guid usuarioId, GuardarDeliveryRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre del delivery es obligatorio.");

        var delivery = new Delivery
        {
            SucursalId = sucursalId,
            Nombre = request.Nombre,
            Telefono = request.Telefono,
            Activo = request.Activo,
            LimiteSaldoPendiente = request.LimiteSaldoPendiente,
            CreadoPorUsuarioId = usuarioId
        };

        db.Deliveries.Add(delivery);
        await db.SaveChangesAsync(ct);
        return delivery.Id;
    }

    public async Task ActualizarAsync(Guid sucursalId, Guid deliveryId, GuardarDeliveryRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre del delivery es obligatorio.");

        var delivery = await db.Deliveries.FirstOrDefaultAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El delivery no existe.");

        delivery.Nombre = request.Nombre;
        delivery.Telefono = request.Telefono;
        delivery.Activo = request.Activo;
        delivery.LimiteSaldoPendiente = request.LimiteSaldoPendiente;

        await db.SaveChangesAsync(ct);
    }

    public async Task DesactivarAsync(Guid sucursalId, Guid deliveryId, CancellationToken ct = default)
    {
        var delivery = await db.Deliveries.FirstOrDefaultAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El delivery no existe.");

        if (delivery.SaldoPendiente > 0)
            throw new InvalidOperationException("No se puede desactivar un delivery con saldo pendiente.");

        delivery.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    // Consulta de solo lectura sobre Facturas — no modifica ni pasa por FacturaAppService,
    // el módulo de ventas/facturación se mantiene intacto.
    public async Task<List<FacturaAsignableDto>> BuscarFacturasAsignablesAsync(Guid sucursalId, string? texto, CancellationToken ct = default)
    {
        // Solo facturas emitidas desde una caja actualmente activa de esta sucursal — una
        // caja dada de baja no debería seguir generando facturas asignables a deliveries.
        var cajasActivasIds = db.Cajas.Where(c => c.SucursalId == sucursalId && c.Activa).Select(c => c.Id);
        var turnosDeCajasActivasIds = db.TurnosCaja.Where(t => cajasActivasIds.Contains(t.CajaId)).Select(t => t.Id);

        // Solo facturas de HOY (calendario RD) y que todavía no estén asignadas a ningún
        // delivery — asignar una factura vieja o ya asignada no tiene sentido de negocio
        // (el delivery se despacha el mismo día que se factura).
        var desdeHoy = HorarioRd.HoyUtc();
        var hastaHoy = desdeHoy.AddDays(1);

        var query = db.Facturas.Where(f =>
            f.SucursalId == sucursalId &&
            turnosDeCajasActivasIds.Contains(f.CajaTurnoId) &&
            f.FechaEmision >= desdeHoy && f.FechaEmision < hastaHoy &&
            !db.FacturasDelivery.Any(fd => fd.FacturaId == f.Id && !fd.Quitada));

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(f =>
                (f.NumeroFactura != null && EF.Functions.Like(f.NumeroFactura, $"%{texto}%")) ||
                (f.NumeroNcf != null && EF.Functions.Like(f.NumeroNcf, $"%{texto}%")));

        var consulta =
            from f in query
            join c in db.Clientes on f.ClienteId equals c.Id into clientes
            from c in clientes.DefaultIfEmpty()
            orderby f.FechaEmision descending
            select new FacturaAsignableDto
            {
                FacturaId = f.Id,
                NumeroFactura = f.NumeroFactura,
                NumeroNcf = f.NumeroNcf,
                FechaEmision = f.FechaEmision,
                Total = f.Total,
                YaAsignada = false,
                DeliveryNombreActual = null,
                ClienteNombre = f.ClienteNombre,
                ClienteTelefono = c != null ? c.Telefono : null
            };

        return await consulta.Take(20).ToListAsync(ct);
    }

    public async Task AsignarFacturaAsync(Guid sucursalId, Guid deliveryId, Guid usuarioId, AsignarFacturaRequestDto request, CancellationToken ct = default)
    {
        var delivery = await db.Deliveries.FirstOrDefaultAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El delivery no existe.");

        if (!delivery.Activo)
            throw new InvalidOperationException("El delivery está inactivo.");

        var factura = await db.Facturas.FirstOrDefaultAsync(f => f.Id == request.FacturaId && f.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La factura no existe.");

        var yaAsignada = await db.FacturasDelivery.AnyAsync(fd => fd.FacturaId == factura.Id && !fd.Quitada, ct);
        if (yaAsignada)
            throw new InvalidOperationException("Esta factura ya está asignada a un delivery.");

        if (request.MontoDelivery < 0)
            throw new InvalidOperationException("El monto del delivery no puede ser negativo.");

        if (delivery.LimiteSaldoPendiente is decimal limite && delivery.SaldoPendiente + factura.Total > limite)
            throw new InvalidOperationException(
                $"Esta asignación superaría el límite de saldo pendiente de {delivery.Nombre} " +
                $"(límite: {limite:N2}, saldo actual: {delivery.SaldoPendiente:N2}).");

        db.FacturasDelivery.Add(new FacturaDelivery
        {
            DeliveryId = delivery.Id,
            FacturaId = factura.Id,
            SucursalId = sucursalId,
            MontoFactura = factura.Total,
            MontoDelivery = request.MontoDelivery,
            AsignadoPorUsuarioId = usuarioId
        });

        // El monto del delivery (flete) no se suma: es dinero que el repartidor se queda,
        // no parte de lo que debe devolver al negocio.
        delivery.SaldoPendiente += factura.Total;

        await db.SaveChangesAsync(ct);
    }

    public async Task QuitarAsignacionAsync(
        Guid sucursalId, Guid deliveryId, Guid facturaDeliveryId, Guid usuarioId, QuitarAsignacionRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new InvalidOperationException("El motivo es obligatorio.");

        var delivery = await db.Deliveries.FirstOrDefaultAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El delivery no existe.");

        var asignacion = await db.FacturasDelivery.FirstOrDefaultAsync(fd => fd.Id == facturaDeliveryId && fd.DeliveryId == deliveryId && !fd.Quitada, ct)
            ?? throw new InvalidOperationException("La asignación no existe.");

        // Sin clamp a 0: si ya se habían abonado más de lo que queda tras quitar esta
        // factura, el saldo debe poder quedar temporalmente negativo (crédito a favor del
        // delivery) — forzarlo a 0 aquí le hacía perder el rastro a ese crédito, y si luego
        // se anulaba el abono correspondiente, el saldo volvía a subir sin ninguna factura
        // detrás que lo respaldara (bug real: factura 480 + abono 200 -> saldo 280; quitar
        // factura clampeaba a 0 en vez de -200; anular el abono sumaba 200 -> saldo fantasma
        // de 200 sin facturas asignadas).
        // Se marca Quitada en vez de borrarse: el kardex del delivery conserva el registro
        // (ver FacturaDelivery.Quitada) y la factura queda libre para reasignarse a otro
        // delivery (índice único filtrado a !Quitada).
        asignacion.Quitada = true;
        asignacion.FechaQuitada = DateTime.UtcNow;
        asignacion.QuitadoPorUsuarioId = usuarioId;
        asignacion.MotivoQuitar = request.Motivo;
        delivery.SaldoPendiente -= asignacion.MontoFactura;

        await db.SaveChangesAsync(ct);
        await auditoria.RegistrarAsync(sucursalId, usuarioId, "QuitarAsignacionDelivery", "FacturaDelivery", asignacion.Id, request.Motivo, ct);
    }

    public async Task<ResumenCuentaDeliveryDto> ObtenerResumenCuentaAsync(Guid sucursalId, Guid deliveryId, CancellationToken ct = default)
    {
        var existe = await db.Deliveries.AnyAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct);
        if (!existe)
            throw new InvalidOperationException("El delivery no existe.");

        return new ResumenCuentaDeliveryDto
        {
            TotalFacturado = await db.FacturasDelivery.Where(fd => fd.DeliveryId == deliveryId && !fd.Quitada).SumAsync(fd => fd.MontoFactura, ct),
            TotalAbonado = await db.AbonosDelivery.Where(a => a.DeliveryId == deliveryId && !a.Anulado).SumAsync(a => a.Monto, ct)
        };
    }

    public async Task<ResultadoPaginado<FacturaDeliveryDto>> ListarFacturasAsignadasAsync(
        Guid sucursalId, Guid deliveryId, DateTime? desde, DateTime? hasta, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        var existe = await db.Deliveries.AnyAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct);
        if (!existe)
            throw new InvalidOperationException("El delivery no existe.");

        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 200);

        var query =
            from fd in db.FacturasDelivery
            join f in db.Facturas on fd.FacturaId equals f.Id
            join u in db.Usuarios on fd.AsignadoPorUsuarioId equals u.Id
            join uq in db.Usuarios on fd.QuitadoPorUsuarioId equals uq.Id into quitadores
            from uq in quitadores.DefaultIfEmpty()
            join c in db.Clientes on f.ClienteId equals c.Id into clientes
            from c in clientes.DefaultIfEmpty()
            where fd.DeliveryId == deliveryId &&
                  (desde == null || fd.FechaAsignacion >= desde) &&
                  (hasta == null || fd.FechaAsignacion <= hasta)
            select new FacturaDeliveryDto
            {
                Id = fd.Id,
                FacturaId = fd.FacturaId,
                NumeroFactura = f.NumeroFactura,
                NumeroNcf = f.NumeroNcf,
                FechaEmision = f.FechaEmision,
                MontoFactura = fd.MontoFactura,
                MontoDelivery = fd.MontoDelivery,
                FechaAsignacion = fd.FechaAsignacion,
                AsignadoPorNombre = u.Nombre,
                ClienteNombre = f.ClienteNombre,
                ClienteTelefono = c != null ? c.Telefono : null,
                Quitada = fd.Quitada,
                FechaQuitada = fd.FechaQuitada,
                QuitadoPorNombre = uq != null ? uq.Nombre : null,
                MotivoQuitar = fd.MotivoQuitar
            };

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(fd => fd.FechaAsignacion)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new ResultadoPaginado<FacturaDeliveryDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = total };
    }

    public async Task<ResultadoPaginado<AbonoDeliveryDto>> ListarAbonosAsync(
        Guid sucursalId, Guid deliveryId, DateTime? desde, DateTime? hasta, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        var existe = await db.Deliveries.AnyAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct);
        if (!existe)
            throw new InvalidOperationException("El delivery no existe.");

        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 200);

        var query =
            from a in db.AbonosDelivery
            join m in db.MetodosPago on a.MetodoPagoId equals m.Id
            join u in db.Usuarios on a.CreadoPorUsuarioId equals u.Id
            join ua in db.Usuarios on a.AnuladoPorUsuarioId equals ua.Id into anuladores
            from ua in anuladores.DefaultIfEmpty()
            where a.DeliveryId == deliveryId &&
                  (desde == null || a.FechaPago >= desde) &&
                  (hasta == null || a.FechaPago <= hasta)
            select new AbonoDeliveryDto
            {
                Id = a.Id,
                Monto = a.Monto,
                FechaPago = a.FechaPago,
                MetodoPagoNombre = m.Nombre,
                NumeroComprobante = a.NumeroComprobante,
                RegistradoPorNombre = u.Nombre,
                Anulado = a.Anulado,
                FechaAnulacion = a.FechaAnulacion,
                AnuladoPorNombre = ua != null ? ua.Nombre : null,
                MotivoAnulacion = a.MotivoAnulacion
            };

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.FechaPago)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new ResultadoPaginado<AbonoDeliveryDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = total };
    }

    public async Task RegistrarAbonoAsync(Guid sucursalId, Guid deliveryId, Guid usuarioId, RegistrarAbonoDeliveryRequestDto request, CancellationToken ct = default)
    {
        var delivery = await db.Deliveries.FirstOrDefaultAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El delivery no existe.");

        if (request.Monto <= 0 || request.Monto > delivery.SaldoPendiente)
            throw new InvalidOperationException("El monto del abono no es válido para el saldo pendiente.");

        var metodoPago = await db.MetodosPago.FirstOrDefaultAsync(m => m.Id == request.MetodoPagoId, ct)
            ?? throw new InvalidOperationException("El método de pago no existe.");

        if (metodoPago.RequiereComprobante && string.IsNullOrWhiteSpace(request.NumeroComprobante))
            throw new InvalidOperationException($"El método de pago \"{metodoPago.Nombre}\" requiere número de comprobante.");

        db.AbonosDelivery.Add(new AbonoDelivery
        {
            DeliveryId = delivery.Id,
            SucursalId = sucursalId,
            Monto = request.Monto,
            MetodoPagoId = request.MetodoPagoId,
            CreadoPorUsuarioId = usuarioId,
            NumeroComprobante = metodoPago.RequiereComprobante ? request.NumeroComprobante : null
        });

        delivery.SaldoPendiente -= request.Monto;

        await db.SaveChangesAsync(ct);
    }

    public async Task AnularAbonoAsync(Guid sucursalId, Guid deliveryId, Guid abonoId, Guid usuarioId, AnularAbonoDeliveryRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new InvalidOperationException("El motivo de la anulación es obligatorio.");

        var delivery = await db.Deliveries.FirstOrDefaultAsync(d => d.Id == deliveryId && d.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El delivery no existe.");

        var abono = await db.AbonosDelivery.FirstOrDefaultAsync(a => a.Id == abonoId && a.DeliveryId == deliveryId, ct)
            ?? throw new InvalidOperationException("El abono no existe.");

        if (abono.Anulado)
            throw new InvalidOperationException("Este abono ya fue anulado.");

        abono.Anulado = true;
        abono.FechaAnulacion = DateTime.UtcNow;
        abono.AnuladoPorUsuarioId = usuarioId;
        abono.MotivoAnulacion = request.Motivo;

        delivery.SaldoPendiente += abono.Monto;

        await db.SaveChangesAsync(ct);
        await auditoria.RegistrarAsync(sucursalId, usuarioId, "AnulacionAbono", "AbonoDelivery", abono.Id, request.Motivo, ct);
    }

    // Movimiento del período (cantidad/monto facturado y abonado entre desde/hasta) +
    // estado ACTUAL de la cuenta (saldo y antigüedad) — mezclar ambos es intencional: el
    // período dice qué tan activo estuvo el delivery, el estado actual dice si quedó
    // debiendo algo, sin importar cuándo se originó esa deuda.
    public async Task<List<ReporteDesempenoDeliveryDto>> ObtenerReporteDesempenoAsync(
        Guid sucursalId, DateTime? desde, DateTime? hasta, CancellationToken ct = default)
    {
        var deliveries = await db.Deliveries.Where(d => d.SucursalId == sucursalId).OrderBy(d => d.Nombre).ToListAsync(ct);
        var resultado = new List<ReporteDesempenoDeliveryDto>();

        foreach (var delivery in deliveries)
        {
            var facturasQuery = db.FacturasDelivery.Where(fd => fd.DeliveryId == delivery.Id && !fd.Quitada);
            if (desde is not null) facturasQuery = facturasQuery.Where(fd => fd.FechaAsignacion >= desde.Value.Date);
            if (hasta is not null) facturasQuery = facturasQuery.Where(fd => fd.FechaAsignacion < hasta.Value.Date.AddDays(1));

            var abonosQuery = db.AbonosDelivery.Where(a => a.DeliveryId == delivery.Id && !a.Anulado);
            if (desde is not null) abonosQuery = abonosQuery.Where(a => a.FechaPago >= desde.Value.Date);
            if (hasta is not null) abonosQuery = abonosQuery.Where(a => a.FechaPago < hasta.Value.Date.AddDays(1));

            resultado.Add(new ReporteDesempenoDeliveryDto
            {
                DeliveryId = delivery.Id,
                DeliveryNombre = delivery.Nombre,
                CantidadEntregas = await facturasQuery.CountAsync(ct),
                MontoTotalFacturado = await facturasQuery.SumAsync(fd => (decimal?)fd.MontoFactura, ct) ?? 0,
                MontoTotalAbonado = await abonosQuery.SumAsync(a => (decimal?)a.Monto, ct) ?? 0,
                SaldoPendienteActual = delivery.SaldoPendiente,
                DiasAntiguedadSaldo = delivery.SaldoPendiente > 0 ? await CalcularDiasAntiguedadSaldoAsync(delivery.Id, ct) : null
            });
        }

        return resultado;
    }
}
