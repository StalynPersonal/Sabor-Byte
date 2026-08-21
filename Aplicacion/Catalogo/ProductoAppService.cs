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
}
