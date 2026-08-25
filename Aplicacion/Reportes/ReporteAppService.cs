using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Reportes.Dtos;
using SaborByte.Dominio.Caja;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Comun;
using SaborByte.Dominio.CxcCxp;

namespace SaborByte.Aplicacion.Reportes;

public class ReporteAppService(IAppDbContext db)
{
    // Comparativo de ventas entre las sucursales que el usuario tiene permitidas
    // (ver sección 5 del plan: "consolidación de reportes por sucursal/central").
    public async Task<ReporteVentasConsolidadoDto> VentasPorSucursalAsync(
        ReporteVentasRequestDto request, CancellationToken ct = default)
    {
        var sucursales = await db.Sucursales
            .Where(s => request.SucursalesIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Nombre, ct);

        var agregados = await db.Facturas
            .Where(f => request.SucursalesIds.Contains(f.SucursalId) &&
                        f.FechaEmision >= request.Desde && f.FechaEmision <= request.Hasta)
            .GroupBy(f => f.SucursalId)
            .Select(g => new
            {
                SucursalId = g.Key,
                Cantidad = g.Count(),
                Total = g.Sum(f => f.Total),
                Itbis = g.Sum(f => f.Itbis)
            })
            .ToListAsync(ct);

        var porSucursal = request.SucursalesIds.Select(id =>
        {
            var datos = agregados.FirstOrDefault(a => a.SucursalId == id);
            var cantidad = datos?.Cantidad ?? 0;
            var total = datos?.Total ?? 0m;

            return new ReporteVentasPorSucursalDto
            {
                SucursalId = id,
                NombreSucursal = sucursales.GetValueOrDefault(id, "(desconocida)"),
                CantidadFacturas = cantidad,
                TotalVendido = total,
                TotalItbis = datos?.Itbis ?? 0m,
                TicketPromedio = cantidad > 0 ? total / cantidad : 0m
            };
        }).ToList();

        return new ReporteVentasConsolidadoDto
        {
            PorSucursal = porSucursal,
            TotalConsolidado = porSucursal.Sum(p => p.TotalVendido)
        };
    }

    public async Task<List<VentaPorProductoDto>> VentasPorProductoAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var agregados = await db.FacturaDetalles
            .Where(d => d.Factura!.SucursalId == sucursalId &&
                        d.Factura.FechaEmision >= rango.Desde && d.Factura.FechaEmision <= rango.Hasta)
            .GroupBy(d => new { d.ProductoId, d.NombreProducto })
            .Select(g => new
            {
                g.Key.ProductoId,
                g.Key.NombreProducto,
                Cantidad = g.Sum(d => d.Cantidad),
                Total = g.Sum(d => d.Total)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync(ct);

        var productoIds = agregados.Select(a => a.ProductoId).ToList();
        var costos = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.CostoUnitario, ct);

        return agregados.Select(a =>
        {
            var costoUnitario = costos.GetValueOrDefault(a.ProductoId);
            return new VentaPorProductoDto
            {
                ProductoId = a.ProductoId,
                NombreProducto = a.NombreProducto,
                CantidadVendida = a.Cantidad,
                TotalVendido = a.Total,
                UtilidadEstimada = a.Total - (costoUnitario * a.Cantidad)
            };
        }).ToList();
    }

    // "Hora pico": cantidad de facturas y total vendido agrupado por hora del día,
    // para identificar los momentos de mayor demanda.
    public async Task<List<VentaPorHoraDto>> VentasPorHoraAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var facturas = await db.Facturas
            .Where(f => f.SucursalId == sucursalId && f.FechaEmision >= rango.Desde && f.FechaEmision <= rango.Hasta)
            .Select(f => new { f.FechaEmision, f.Total })
            .ToListAsync(ct);

        // Hora de RD, no UTC (ver HorarioRd) — sin esto, el gráfico de "hora pico" mostraba
        // la hora del servidor, 4 horas adelantada respecto a la hora real del negocio.
        return facturas
            .GroupBy(f => HorarioRd.AHoraLocal(f.FechaEmision).Hour)
            .Select(g => new VentaPorHoraDto
            {
                Hora = g.Key,
                CantidadFacturas = g.Count(),
                TotalVendido = g.Sum(f => f.Total)
            })
            .OrderBy(v => v.Hora)
            .ToList();
    }

    // Ventas agrupadas por día calendario — para la pestaña "Ventas resumidas por fecha".
    public async Task<List<VentaResumenDiaDto>> VentasResumenPorDiaAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var facturas = await db.Facturas
            .Where(f => f.SucursalId == sucursalId && f.FechaEmision >= rango.Desde && f.FechaEmision <= rango.Hasta)
            .Select(f => new { f.FechaEmision, f.Total, f.Itbis })
            .ToListAsync(ct);

        // Agrupa por día calendario de RD, no por día UTC (ver HorarioRd) — una venta de
        // las 9pm hora RD es UTC 1am del día siguiente, y sin esta conversión aparecía
        // agrupada bajo la fecha equivocada.
        return facturas
            .GroupBy(f => HorarioRd.AHoraLocal(f.FechaEmision).Date)
            .Select(g =>
            {
                var cantidad = g.Count();
                var total = g.Sum(f => f.Total);
                return new VentaResumenDiaDto
                {
                    Fecha = g.Key,
                    CantidadFacturas = cantidad,
                    TotalVendido = total,
                    TotalItbis = g.Sum(f => f.Itbis),
                    TicketPromedio = cantidad > 0 ? total / cantidad : 0m
                };
            })
            .OrderBy(v => v.Fecha)
            .ToList();
    }

    // Detalle factura por factura — para la pestaña "Ventas detalle por fecha".
    public async Task<List<VentaDetalleDto>> VentasDetalleAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var facturas = await db.Facturas
            .Include(f => f.Pagos).ThenInclude(p => p.MetodoPago)
            .Where(f => f.SucursalId == sucursalId && f.FechaEmision >= rango.Desde && f.FechaEmision <= rango.Hasta)
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync(ct);

        var cajeroIds = facturas.Select(f => f.CreadoPorUsuarioId).Distinct().ToList();
        var nombresCajero = await db.Usuarios
            .Where(u => cajeroIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Nombre, ct);

        var comandaIds = facturas.Where(f => f.ComandaId is not null).Select(f => f.ComandaId!.Value).ToList();
        var meserosPorComanda = await db.Comandas
            .Where(c => comandaIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.NombreMesero, ct);

        return facturas.Select(f => new VentaDetalleDto
        {
            FacturaId = f.Id,
            NumeroFactura = f.NumeroFactura,
            NumeroNcf = f.NumeroNcf,
            FechaEmision = f.FechaEmision,
            ClienteNombre = f.ClienteNombre,
            Subtotal = f.Subtotal,
            Itbis = f.Itbis,
            Descuento = f.Descuento,
            Total = f.Total,
            FormasPago = string.Join(", ", f.Pagos.Select(p => $"{p.MetodoPago?.Nombre ?? "?"}: RD$ {p.Monto:0.00}")),
            CajeroNombre = nombresCajero.GetValueOrDefault(f.CreadoPorUsuarioId, "(desconocido)"),
            MeseroNombre = f.ComandaId is Guid comandaId ? meserosPorComanda.GetValueOrDefault(comandaId) : null
        }).ToList();
    }

    // Resumen agrupado por cajero — para la pestaña "Ventas por Cajero".
    public async Task<List<VentaPorCajeroDto>> VentasPorCajeroAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var agregados = await db.Facturas
            .Where(f => f.SucursalId == sucursalId && f.FechaEmision >= rango.Desde && f.FechaEmision <= rango.Hasta)
            .GroupBy(f => f.CreadoPorUsuarioId)
            .Select(g => new { CajeroId = g.Key, Cantidad = g.Count(), Total = g.Sum(f => f.Total) })
            .ToListAsync(ct);

        var cajeroIds = agregados.Select(a => a.CajeroId).ToList();
        var nombres = await db.Usuarios.Where(u => cajeroIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Nombre, ct);

        return agregados.Select(a => new VentaPorCajeroDto
        {
            CajeroId = a.CajeroId,
            NombreCajero = nombres.GetValueOrDefault(a.CajeroId, "(desconocido)"),
            CantidadFacturas = a.Cantidad,
            TotalVendido = a.Total,
            TicketPromedio = a.Cantidad > 0 ? a.Total / a.Cantidad : 0m
        }).OrderByDescending(v => v.TotalVendido).ToList();
    }

    // Resumen agrupado por mesero — para la pestaña "Ventas por Mesero". Solo incluye
    // ventas que vinieron de una comanda con mesero asignado (ver VentaDetalleDto).
    public async Task<List<VentaPorMeseroDto>> VentasPorMeseroAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var agregados = await (
                from f in db.Facturas
                join c in db.Comandas on f.ComandaId equals c.Id
                where f.SucursalId == sucursalId && f.FechaEmision >= rango.Desde && f.FechaEmision <= rango.Hasta
                      && c.MeseroId != null
                group f by new { MeseroId = c.MeseroId!.Value, c.NombreMesero } into g
                select new VentaPorMeseroDto
                {
                    MeseroId = g.Key.MeseroId,
                    NombreMesero = g.Key.NombreMesero ?? "(desconocido)",
                    CantidadFacturas = g.Count(),
                    TotalVendido = g.Sum(f => f.Total)
                }
            )
            .ToListAsync(ct);

        foreach (var item in agregados)
            item.TicketPromedio = item.CantidadFacturas > 0 ? item.TotalVendido / item.CantidadFacturas : 0m;

        return agregados.OrderByDescending(v => v.TotalVendido).ToList();
    }

    // Totales cobrados por cada método de pago — para la pestaña "Ventas por método de pago".
    public async Task<List<VentaPorMetodoPagoDto>> VentasPorMetodoPagoAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var pagos = await db.FacturaPagos
            .Include(p => p.MetodoPago)
            .Where(p => p.Factura!.SucursalId == sucursalId &&
                        p.Factura.FechaEmision >= rango.Desde && p.Factura.FechaEmision <= rango.Hasta)
            .ToListAsync(ct);

        return pagos
            .GroupBy(p => new { p.MetodoPagoId, Nombre = p.MetodoPago?.Nombre ?? "(desconocido)" })
            .Select(g => new VentaPorMetodoPagoDto
            {
                MetodoPagoId = g.Key.MetodoPagoId,
                NombreMetodo = g.Key.Nombre,
                CantidadPagos = g.Count(),
                TotalCobrado = g.Sum(p => p.Monto)
            })
            .OrderByDescending(v => v.TotalCobrado)
            .ToList();
    }

    // Totales vendidos agrupados por categoría — para el gráfico de dona del dashboard.
    public async Task<List<VentaPorCategoriaDto>> VentasPorCategoriaAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var detalles = await db.FacturaDetalles
            .Where(d => d.Factura!.SucursalId == sucursalId &&
                        d.Factura.FechaEmision >= rango.Desde && d.Factura.FechaEmision <= rango.Hasta)
            .Select(d => new { d.ProductoId, d.Total })
            .ToListAsync(ct);

        var productoIds = detalles.Select(d => d.ProductoId).Distinct().ToList();
        var categoriasPorProducto = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .Select(p => new { p.Id, p.CategoriaId })
            .ToDictionaryAsync(p => p.Id, p => p.CategoriaId, ct);

        var categoriaIds = categoriasPorProducto.Values.Distinct().ToList();
        var nombresCategoria = await db.Categorias
            .Where(c => categoriaIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Nombre, ct);

        return detalles
            .Where(d => categoriasPorProducto.ContainsKey(d.ProductoId))
            .GroupBy(d => categoriasPorProducto[d.ProductoId])
            .Select(g => new VentaPorCategoriaDto
            {
                CategoriaId = g.Key,
                NombreCategoria = nombresCategoria.GetValueOrDefault(g.Key, "(categoría eliminada)"),
                TotalVendido = g.Sum(d => d.Total)
            })
            .OrderByDescending(v => v.TotalVendido)
            .ToList();
    }

    // Kardex del rango — para la pestaña "Movimientos de inventario".
    public async Task<List<MovimientoInventarioReporteDto>> MovimientosInventarioAsync(
        Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var movimientos = await db.MovimientosInventario
            .Where(m => m.SucursalId == sucursalId && m.FechaHora >= rango.Desde && m.FechaHora <= rango.Hasta)
            .OrderByDescending(m => m.FechaHora)
            .Take(5000)
            .ToListAsync(ct);

        var productoIds = movimientos.Select(m => m.ProductoId).Distinct().ToList();
        var nombres = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Nombre, ct);

        return movimientos.Select(m => new MovimientoInventarioReporteDto
        {
            FechaHora = m.FechaHora,
            NombreProducto = nombres.GetValueOrDefault(m.ProductoId, "(producto eliminado)"),
            Tipo = m.Tipo.ToString(),
            Cantidad = m.Cantidad,
            SaldoResultante = m.SaldoResultante,
            Nota = m.Nota
        }).ToList();
    }

    // Cuentas por cobrar pendientes — para la pestaña "CxC pendiente".
    public async Task<List<CuentaPendienteDto>> CxCPendientesAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var cuentas = await db.CuentasPorCobrar
            .Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada)
            .OrderBy(c => c.FechaVencimiento)
            .ToListAsync(ct);

        var clienteIds = cuentas.Select(c => c.ClienteId).Distinct().ToList();
        var nombres = await db.Clientes
            .Where(c => clienteIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.NombreORazonSocial, ct);

        return cuentas.Select(c => new CuentaPendienteDto
        {
            CuentaId = c.Id,
            Nombre = nombres.GetValueOrDefault(c.ClienteId, "(cliente eliminado)"),
            MontoOriginal = c.MontoOriginal,
            SaldoPendiente = c.SaldoPendiente,
            FechaVencimiento = c.FechaVencimiento,
            Estado = c.Estado.ToString()
        }).ToList();
    }

    // Cuentas por pagar pendientes — para la pestaña "CxP pendiente".
    public async Task<List<CuentaPendienteDto>> CxPPendientesAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var cuentas = await db.CuentasPorPagar
            .Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada)
            .OrderBy(c => c.FechaVencimiento)
            .ToListAsync(ct);

        var proveedorIds = cuentas.Select(c => c.ProveedorId).Distinct().ToList();
        var nombres = await db.Proveedores
            .Where(p => proveedorIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.NombreORazonSocial, ct);

        return cuentas.Select(c => new CuentaPendienteDto
        {
            CuentaId = c.Id,
            Nombre = nombres.GetValueOrDefault(c.ProveedorId, "(proveedor eliminado)"),
            MontoOriginal = c.MontoOriginal,
            SaldoPendiente = c.SaldoPendiente,
            FechaVencimiento = c.FechaVencimiento,
            Estado = c.Estado.ToString()
        }).ToList();
    }

    // Pagos de cuentas por cobrar dentro de un rango de fechas — para el reporte "Pagos CxC recibidos".
    public async Task<List<PagoCuentaReporteDto>> CxCPagosAsync(Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var pagos = await (
                from p in db.PagosCxC
                join c in db.CuentasPorCobrar on p.CuentaPorCobrarId equals c.Id
                join cl in db.Clientes on c.ClienteId equals cl.Id
                join m in db.MetodosPago on p.MetodoPagoId equals m.Id
                join u in db.Usuarios on p.CreadoPorUsuarioId equals u.Id
                where c.SucursalId == sucursalId && p.FechaPago >= rango.Desde && p.FechaPago <= rango.Hasta
                orderby p.FechaPago descending
                select new PagoCuentaReporteDto
                {
                    PagoId = p.Id,
                    Nombre = cl.NombreORazonSocial,
                    FechaPago = p.FechaPago,
                    Monto = p.Monto,
                    MetodoPagoNombre = m.Nombre,
                    NumeroComprobante = p.NumeroComprobante,
                    RegistradoPorNombre = u.Nombre,
                    Anulado = p.Anulado
                }
            )
            .ToListAsync(ct);

        return pagos;
    }

    // Pagos de cuentas por pagar dentro de un rango de fechas — para el reporte "Pagos CxP realizados".
    public async Task<List<PagoCuentaReporteDto>> CxPPagosAsync(Guid sucursalId, RangoFechasRequestDto rango, CancellationToken ct = default)
    {
        var pagos = await (
                from p in db.PagosCxP
                join c in db.CuentasPorPagar on p.CuentaPorPagarId equals c.Id
                join pr in db.Proveedores on c.ProveedorId equals pr.Id
                join m in db.MetodosPago on p.MetodoPagoId equals m.Id
                join u in db.Usuarios on p.CreadoPorUsuarioId equals u.Id
                where c.SucursalId == sucursalId && p.FechaPago >= rango.Desde && p.FechaPago <= rango.Hasta
                orderby p.FechaPago descending
                select new PagoCuentaReporteDto
                {
                    PagoId = p.Id,
                    Nombre = pr.NombreORazonSocial,
                    FechaPago = p.FechaPago,
                    Monto = p.Monto,
                    MetodoPagoNombre = m.Nombre,
                    NumeroComprobante = p.NumeroComprobante,
                    RegistradoPorNombre = u.Nombre,
                    Anulado = p.Anulado
                }
            )
            .ToListAsync(ct);

        return pagos;
    }

    // KPIs del día para el dashboard de Inicio: ventas de hoy, turnos abiertos, CxC/CxP
    // pendientes y alertas de stock bajo, todo scopeado a una sola sucursal.
    public async Task<DashboardResumenDto> ObtenerDashboardAsync(Guid sucursalId, CancellationToken ct = default)
    {
        // "Hoy" según el calendario de República Dominicana (UTC-4, sin horario de
        // verano), no el día UTC del servidor — sin esto, ventas de la tarde/noche (hora
        // RD) quedaban fuera de "hoy" porque en UTC ya era el día siguiente.
        var desde = HorarioRd.HoyUtc();
        var hasta = desde.AddDays(1).AddSeconds(-1);
        var rangoHoy = new RangoFechasRequestDto { Desde = desde, Hasta = hasta };

        var totalesHoy = await db.Facturas
            .Where(f => f.SucursalId == sucursalId && f.FechaEmision >= desde && f.FechaEmision <= hasta)
            .Select(f => f.Total)
            .ToListAsync(ct);

        var cantidadHoy = totalesHoy.Count;
        var totalVendidoHoy = totalesHoy.Sum();

        var turnosAbiertos = await db.TurnosCaja
            .CountAsync(t => t.Caja!.SucursalId == sucursalId && t.Estado == EstadoTurnoCaja.Abierto, ct);

        var saldosCxC = await db.CuentasPorCobrar
            .Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada)
            .Select(c => c.SaldoPendiente)
            .ToListAsync(ct);

        var saldosCxP = await db.CuentasPorPagar
            .Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada)
            .Select(c => c.SaldoPendiente)
            .ToListAsync(ct);

        var productosStockBajo = await db.StockPorSucursal
            .CountAsync(s => s.SucursalId == sucursalId && s.StockMinimo != null && s.StockActual < s.StockMinimo, ct);

        var cobradoHoyCxC = await db.PagosCxC
            .Where(p => !p.Anulado && p.FechaPago >= desde && p.FechaPago <= hasta && p.Cuenta!.SucursalId == sucursalId)
            .SumAsync(p => (decimal?)p.Monto, ct) ?? 0m;

        var pagadoHoyCxP = await db.PagosCxP
            .Where(p => !p.Anulado && p.FechaPago >= desde && p.FechaPago <= hasta && p.Cuenta!.SucursalId == sucursalId)
            .SumAsync(p => (decimal?)p.Monto, ct) ?? 0m;

        var cxcVencidas = await db.CuentasPorCobrar
            .Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada && c.FechaVencimiento < desde)
            .Select(c => c.SaldoPendiente)
            .ToListAsync(ct);

        var cxpVencidas = await db.CuentasPorPagar
            .Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada && c.FechaVencimiento < desde)
            .Select(c => c.SaldoPendiente)
            .ToListAsync(ct);

        var notasCreditoHoy = await db.NotasCredito
            .Where(n => n.SucursalId == sucursalId && n.FechaEmision >= desde && n.FechaEmision <= hasta)
            .Select(n => n.Monto)
            .ToListAsync(ct);

        return new DashboardResumenDto
        {
            VentasHoyTotal = totalVendidoHoy,
            VentasHoyCantidadFacturas = cantidadHoy,
            TicketPromedioHoy = cantidadHoy > 0 ? totalVendidoHoy / cantidadHoy : 0m,
            TurnosAbiertos = turnosAbiertos,
            CxCPendienteCantidad = saldosCxC.Count,
            CxCPendienteTotal = saldosCxC.Sum(),
            CxPPendienteCantidad = saldosCxP.Count,
            CxPPendienteTotal = saldosCxP.Sum(),
            ProductosStockBajo = productosStockBajo,
            CobradoHoyCxC = cobradoHoyCxC,
            PagadoHoyCxP = pagadoHoyCxP,
            CuentasVencidasCantidad = cxcVencidas.Count + cxpVencidas.Count,
            CuentasVencidasTotal = cxcVencidas.Sum() + cxpVencidas.Sum(),
            NotasCreditoHoyCantidad = notasCreditoHoy.Count,
            NotasCreditoHoyTotal = notasCreditoHoy.Sum(),
            VentasPorHoraHoy = await VentasPorHoraAsync(sucursalId, rangoHoy, ct),
            TopProductosHoy = (await VentasPorProductoAsync(sucursalId, rangoHoy, ct)).Take(5).ToList()
        };
    }
}
