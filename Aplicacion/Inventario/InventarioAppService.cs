using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Inventario;

namespace SaborByte.Aplicacion.Inventario;

public class InventarioAppService(IAppDbContext db)
{
    // Recorre la receta efectiva de un producto vendible (BOM directo, o si es un
    // combo, la unión de las recetas de sus componentes) y descuenta cada insumo
    // del kardex. ingredientesExcluidosIds: ingredientes que el cliente pidió sin
    // (ej. "sin tomate") — solo aplica al BOM directo, no al interior de un combo.
    public async Task DescontarPorVentaAsync(
        Guid sucursalId,
        Guid productoVendibleId,
        decimal cantidadVendida,
        Guid referenciaId,
        IReadOnlyCollection<Guid> ingredientesExcluidosIds,
        Guid? usuarioId,
        CancellationToken ct = default)
    {
        var receta = await ObtenerRecetaEfectivaAsync(productoVendibleId, cantidadVendida, ingredientesExcluidosIds, ct);

        foreach (var (insumoId, cantidad) in receta)
        {
            await RegistrarMovimientoAsync(
                sucursalId, insumoId, TipoMovimientoInventario.ConsumoVenta,
                -cantidad, referenciaId, usuarioId, ct);
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
        var receta = await ObtenerRecetaEfectivaAsync(productoVendibleId, cantidadCancelada, ingredientesExcluidosIds, ct);

        foreach (var (insumoId, cantidad) in receta)
        {
            await RegistrarMovimientoAsync(
                sucursalId, insumoId, TipoMovimientoInventario.ReversoCancelacion,
                cantidad, referenciaId, usuarioId, ct);
        }
    }

    private async Task<List<(Guid InsumoId, decimal Cantidad)>> ObtenerRecetaEfectivaAsync(
        Guid productoId, decimal cantidadVendida, IReadOnlyCollection<Guid> ingredientesExcluidosIds, CancellationToken ct)
    {
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == productoId, ct);

        if (producto is { EsCombo: true })
        {
            var componentes = await db.ComboItems
                .Where(c => c.ComboId == productoId)
                .ToListAsync(ct);

            var resultado = new List<(Guid, decimal)>();
            foreach (var componente in componentes)
            {
                // Los componentes de un combo no llevan exclusión de ingredientes propia en v1.
                var subReceta = await ObtenerRecetaEfectivaAsync(
                    componente.ProductoIncluidoId, componente.Cantidad * cantidadVendida, [], ct);
                resultado.AddRange(subReceta);
            }
            return resultado;
        }

        var receta = await db.ProductoIngredientes
            .Where(pi => pi.ProductoId == productoId && !ingredientesExcluidosIds.Contains(pi.InsumoId))
            .ToListAsync(ct);

        return receta.Select(pi => (pi.InsumoId, pi.CantidadUsada * cantidadVendida)).ToList();
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
