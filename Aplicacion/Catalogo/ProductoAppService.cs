using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Catalogo.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo;

public class ProductoAppService(IAppDbContext db)
{
    // Búsqueda rápida para Caja/Mesero: por código de barra exacto o por coincidencia parcial de descripción.
    public async Task<List<ProductoResumenDto>> BuscarAsync(Guid sucursalId, string texto, CancellationToken ct = default)
    {
        var query = db.Productos.Where(p =>
            p.SucursalId == sucursalId &&
            p.Activo &&
            p.TipoProducto == TipoProducto.Vendible);

        if (!string.IsNullOrWhiteSpace(texto))
        {
            query = query.Where(p =>
                p.CodigoBarra == texto ||
                EF.Functions.Like(p.Nombre, $"%{texto}%"));
        }

        return await query
            .OrderBy(p => p.Nombre)
            .Take(50)
            .Select(p => new ProductoResumenDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                ImagenUrl = p.ImagenUrl,
                CodigoBarra = p.CodigoBarra,
                Precio = p.Precio,
                AplicaItbis = p.AplicaItbis,
                TipoProducto = p.TipoProducto
            })
            .ToListAsync(ct);
    }

    // Crea un producto Vendible marcado como combo, con sus componentes (otros
    // productos Vendibles). El descuento de inventario se resuelve expandiendo cada
    // componente a su propia receta (ver InventarioAppService.ObtenerRecetaEfectivaAsync).
    public async Task<Guid> CrearComboAsync(Guid sucursalId, CrearComboRequestDto request, CancellationToken ct = default)
    {
        if (request.Componentes.Count == 0)
            throw new InvalidOperationException("El combo debe tener al menos un componente.");

        var componenteIds = request.Componentes.Select(c => c.ProductoIncluidoId).ToList();
        var existentes = await db.Productos
            .Where(p => componenteIds.Contains(p.Id) && p.TipoProducto == TipoProducto.Vendible)
            .Select(p => p.Id)
            .ToListAsync(ct);

        var faltante = componenteIds.Except(existentes).FirstOrDefault();
        if (faltante != Guid.Empty)
            throw new InvalidOperationException($"El producto {faltante} no existe o no es vendible.");

        var combo = new Producto
        {
            SucursalId = sucursalId,
            Nombre = request.Nombre,
            Precio = request.Precio,
            CategoriaId = request.CategoriaId,
            TipoProducto = TipoProducto.Vendible,
            EsCombo = true
        };

        foreach (var componente in request.Componentes)
        {
            combo.ComponentesCombo.Add(new ComboItem
            {
                ComboId = combo.Id,
                ProductoIncluidoId = componente.ProductoIncluidoId,
                Cantidad = componente.Cantidad
            });
        }

        db.Productos.Add(combo);
        await db.SaveChangesAsync(ct);

        return combo.Id;
    }
}
