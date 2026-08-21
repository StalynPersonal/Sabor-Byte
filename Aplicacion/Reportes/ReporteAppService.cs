using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Reportes.Dtos;

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
                UtilidadEstimada = costoUnitario.HasValue ? a.Total - (costoUnitario.Value * a.Cantidad) : null
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

        return facturas
            .GroupBy(f => f.FechaEmision.Hour)
            .Select(g => new VentaPorHoraDto
            {
                Hora = g.Key,
                CantidadFacturas = g.Count(),
                TotalVendido = g.Sum(f => f.Total)
            })
            .OrderBy(v => v.Hora)
            .ToList();
    }
}
