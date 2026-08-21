using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Inventario;

namespace SaborByte.Aplicacion.Inventario;

public class InventarioAppService(IAppDbContext db)
{
    // Recorre la receta (BOM) de un producto vendible y descuenta cada insumo del kardex.
    // ingredientesExcluidosIds: ingredientes que el cliente pidió sin (ej. "sin tomate").
    public async Task DescontarPorVentaAsync(
        Guid sucursalId,
        Guid productoVendibleId,
        decimal cantidadVendida,
        Guid referenciaId,
        IReadOnlyCollection<Guid> ingredientesExcluidosIds,
        Guid? usuarioId,
        CancellationToken ct = default)
    {
        var receta = await db.ProductoIngredientes
            .Where(pi => pi.ProductoId == productoVendibleId && !ingredientesExcluidosIds.Contains(pi.InsumoId))
            .ToListAsync(ct);

        foreach (var linea in receta)
        {
            var cantidadADescontar = linea.CantidadUsada * cantidadVendida;
            await RegistrarMovimientoAsync(
                sucursalId, linea.InsumoId, TipoMovimientoInventario.ConsumoVenta,
                -cantidadADescontar, referenciaId, usuarioId, ct);
        }
    }

    public async Task RevertirPorCancelacionAsync(
        Guid sucursalId,
        Guid productoVendibleId,
        decimal cantidadCancelada,
        Guid referenciaId,
        IReadOnlyCollection<Guid> ingredientesExcluidosIds,
        Guid? usuarioId,
        CancellationToken ct = default)
    {
        var receta = await db.ProductoIngredientes
            .Where(pi => pi.ProductoId == productoVendibleId && !ingredientesExcluidosIds.Contains(pi.InsumoId))
            .ToListAsync(ct);

        foreach (var linea in receta)
        {
            var cantidadARevertir = linea.CantidadUsada * cantidadCancelada;
            await RegistrarMovimientoAsync(
                sucursalId, linea.InsumoId, TipoMovimientoInventario.ReversoCancelacion,
                cantidadARevertir, referenciaId, usuarioId, ct);
        }
    }

    private async Task RegistrarMovimientoAsync(
        Guid sucursalId, Guid insumoId, TipoMovimientoInventario tipo,
        decimal cantidadConSigno, Guid referenciaId, Guid? usuarioId, CancellationToken ct)
    {
        var insumo = await db.Productos.FirstOrDefaultAsync(p => p.Id == insumoId, ct);
        if (insumo is null)
            return;

        insumo.StockActual += cantidadConSigno;

        db.MovimientosInventario.Add(new Dominio.Inventario.MovimientoInventario
        {
            SucursalId = sucursalId,
            ProductoId = insumoId,
            Tipo = tipo,
            Cantidad = cantidadConSigno,
            SaldoResultante = insumo.StockActual,
            ReferenciaId = referenciaId,
            CreadoPorUsuarioId = usuarioId
        });
    }
}
