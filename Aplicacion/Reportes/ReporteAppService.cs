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
}
