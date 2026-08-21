using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Catalogo.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo;

public class CategoriaAppService(IAppDbContext db)
{
    public async Task<List<CategoriaDto>> ListarAsync(Guid sucursalId, CancellationToken ct = default) =>
        await db.Categorias
            .Where(c => c.SucursalId == sucursalId)
            .OrderBy(c => c.Orden).ThenBy(c => c.Nombre)
            .Select(c => new CategoriaDto { Id = c.Id, Nombre = c.Nombre, Orden = c.Orden })
            .ToListAsync(ct);

    public async Task<Guid> CrearAsync(Guid sucursalId, GuardarCategoriaRequestDto request, CancellationToken ct = default)
    {
        var categoria = new Categoria { SucursalId = sucursalId, Nombre = request.Nombre, Orden = request.Orden };
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync(ct);
        return categoria.Id;
    }

    public async Task ActualizarAsync(Guid sucursalId, Guid categoriaId, GuardarCategoriaRequestDto request, CancellationToken ct = default)
    {
        var categoria = await db.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La categoría no existe.");

        categoria.Nombre = request.Nombre;
        categoria.Orden = request.Orden;
        await db.SaveChangesAsync(ct);
    }
}
