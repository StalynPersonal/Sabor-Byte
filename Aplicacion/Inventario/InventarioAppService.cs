using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Inventario.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Inventario;

namespace SaborByte.Aplicacion.Inventario;

// El producto (catálogo) es de toda la empresa, pero el STOCK es por sucursal (ver
// StockSucursal.cs) — la sucursal Norte y la sucursal Sur del mismo insumo llevan
// cada una su propio saldo, independiente.
public class InventarioAppService(IAppDbContext db)
{
    // Entrada manual de stock (ej. llegó mercancía del proveedor). Sin esto, StockActual
    // solo podía bajar por venta — no había forma de reponerlo desde la app (ver sección
    // "Consideraciones Adicionales" del plan, kardex con tipo Entrada nunca usado).
    public async Task<Guid> RegistrarEntradaAsync(
        Guid sucursalId, Guid usuarioId, RegistrarEntradaRequestDto request, CancellationToken ct = default)
    {
        if (request.Cantidad <= 0)
            throw new InvalidOperationException("La cantidad de entrada debe ser mayor a cero.");

        var producto = await ObtenerInsumoAsync(request.ProductoId, ct);
        var stock = await ObtenerOCrearStockAsync(producto.Id, sucursalId, ct);

        stock.StockActual += request.Cantidad;
        if (request.CostoUnitario is > 0)
            producto.CostoUnitario = request.CostoUnitario.Value; // costo de referencia = última compra

        var movimiento = new MovimientoInventario
        {
            SucursalId = sucursalId,
            ProductoId = producto.Id,
            Tipo = TipoMovimientoInventario.Entrada,
            Cantidad = request.Cantidad,
            CostoUnitario = request.CostoUnitario,
            SaldoResultante = stock.StockActual,
            Nota = request.Nota,
            CreadoPorUsuarioId = usuarioId
        };

        db.MovimientosInventario.Add(movimiento);
        await db.SaveChangesAsync(ct);
        return movimiento.Id;
    }

    // Ajuste manual (ej. conteo físico distinto al del sistema, merma, producto dañado).
    // A diferencia de una Entrada, el delta puede ser negativo.
    public async Task<Guid> RegistrarAjusteAsync(
        Guid sucursalId, Guid usuarioId, RegistrarAjusteRequestDto request, CancellationToken ct = default)
    {
        var producto = await ObtenerInsumoAsync(request.ProductoId, ct);
        var stock = await ObtenerOCrearStockAsync(producto.Id, sucursalId, ct);

        var delta = request.NuevoStock - stock.StockActual;
        if (delta == 0)
            throw new InvalidOperationException("El nuevo stock es igual al actual; no hay nada que ajustar.");

        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new InvalidOperationException("El motivo del ajuste es obligatorio.");

        stock.StockActual = request.NuevoStock;

        var movimiento = new MovimientoInventario
        {
            SucursalId = sucursalId,
            ProductoId = producto.Id,
            Tipo = TipoMovimientoInventario.Ajuste,
            Cantidad = delta,
            SaldoResultante = stock.StockActual,
            Nota = request.Motivo,
            CreadoPorUsuarioId = usuarioId
        };

        db.MovimientosInventario.Add(movimiento);
        await db.SaveChangesAsync(ct);
        return movimiento.Id;
    }

    // Configura el mínimo/máximo de alerta de un insumo para una sucursal puntual — cada
    // sucursal puede necesitar un umbral distinto (ver decisión: stock mín/máx es por sucursal).
    public async Task ConfigurarUmbralesAsync(
        Guid sucursalId, Guid productoId, decimal? stockMinimo, decimal? stockMaximo, CancellationToken ct = default)
    {
        await ObtenerInsumoAsync(productoId, ct);
        var stock = await ObtenerOCrearStockAsync(productoId, sucursalId, ct);

        stock.StockMinimo = stockMinimo;
        stock.StockMaximo = stockMaximo;
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<MovimientoInventarioDto>> ListarMovimientosAsync(
        Guid sucursalId, Guid? productoId, CancellationToken ct = default)
    {
        var query = db.MovimientosInventario.Where(m => m.SucursalId == sucursalId);
        if (productoId is not null)
            query = query.Where(m => m.ProductoId == productoId);

        var movimientos = await query.OrderByDescending(m => m.FechaHora).Take(200).ToListAsync(ct);
        var productoIds = movimientos.Select(m => m.ProductoId).Distinct().ToList();
        var nombres = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Nombre, ct);

        return movimientos.Select(m => new MovimientoInventarioDto
        {
            Id = m.Id,
            ProductoId = m.ProductoId,
            NombreProducto = nombres.GetValueOrDefault(m.ProductoId, "(producto eliminado)"),
            Tipo = m.Tipo,
            Cantidad = m.Cantidad,
            SaldoResultante = m.SaldoResultante,
            Nota = m.Nota,
            FechaHora = m.FechaHora
        }).ToList();
    }

    private async Task<Producto> ObtenerInsumoAsync(Guid productoId, CancellationToken ct)
    {
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == productoId, ct)
            ?? throw new InvalidOperationException("El producto no existe.");

        // Insumo, o Vendible de reventa marcado Inventariable (ej. agua embotellada) —
        // ambos llevan stock propio (ver Producto.Inventariable).
        if (!producto.Inventariable)
            throw new InvalidOperationException("Este producto no es inventariable — no lleva stock propio.");

        return producto;
    }

    // Defensivo: normalmente el renglón ya existe (se crea al dar de alta el insumo o la
    // sucursal, ver ProductoAppService.CrearAsync / SucursalAppService.CrearAsync), pero
    // por si acaso (datos de antes de este cambio, orden de creación inusual) se crea aquí.
    private async Task<StockSucursal> ObtenerOCrearStockAsync(Guid productoId, Guid sucursalId, CancellationToken ct)
    {
        var stock = await db.StockPorSucursal
            .FirstOrDefaultAsync(s => s.ProductoId == productoId && s.SucursalId == sucursalId, ct);

        if (stock is not null)
            return stock;

        stock = new StockSucursal { ProductoId = productoId, SucursalId = sucursalId };
        db.StockPorSucursal.Add(stock);
        return stock;
    }

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

        // Vendible de reventa (ej. agua embotellada): no tiene receta, se descuenta a sí
        // mismo directo, igual que un Insumo (ver Producto.Inventariable).
        if (producto is { Inventariable: true })
            return [(producto.Id, cantidadVendida)];

        var receta = await db.ProductoIngredientes
            .Where(pi => pi.ProductoId == productoId && !ingredientesExcluidosIds.Contains(pi.InsumoId))
            .ToListAsync(ct);

        return receta.Select(pi => (pi.InsumoId, pi.CantidadUsada * cantidadVendida)).ToList();
    }

    // Punto único donde se toca StockSucursal.StockActual — para un ConsumoVenta, si no
    // alcanza el stock de ESA sucursal se bloquea aquí mismo con el disponible real (no el
    // de otra sucursal). Como todas las líneas de una venta pasan por acá dentro del mismo
    // DbContext antes de guardar, dos líneas que comparten insumo también quedan cubiertas
    // (el descuento de la primera ya se refleja en memoria cuando se valida la segunda).
    private async Task RegistrarMovimientoAsync(
        Guid sucursalId, Guid insumoId, TipoMovimientoInventario tipo,
        decimal cantidadConSigno, Guid referenciaId, Guid? usuarioId, CancellationToken ct)
    {
        var insumo = await db.Productos.FirstOrDefaultAsync(p => p.Id == insumoId, ct);
        if (insumo is null)
            return;

        var stock = await ObtenerOCrearStockAsync(insumoId, sucursalId, ct);

        if (tipo == TipoMovimientoInventario.ConsumoVenta && stock.StockActual + cantidadConSigno < 0)
        {
            throw new InvalidOperationException(
                $"Stock insuficiente de '{insumo.Nombre}' en esta sucursal: disponible {stock.StockActual:0.###}, solicitado {-cantidadConSigno:0.###}.");
        }

        stock.StockActual += cantidadConSigno;

        db.MovimientosInventario.Add(new MovimientoInventario
        {
            SucursalId = sucursalId,
            ProductoId = insumoId,
            Tipo = tipo,
            Cantidad = cantidadConSigno,
            SaldoResultante = stock.StockActual,
            ReferenciaId = referenciaId,
            CreadoPorUsuarioId = usuarioId
        });
    }
}
