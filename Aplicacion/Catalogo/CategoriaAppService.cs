using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Catalogo.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo;

// Catálogo de toda la empresa — ver comentario de clase en Producto.cs.
public class CategoriaAppService(IAppDbContext db)
{
    public async Task<List<CategoriaDto>> ListarAsync(bool incluirInactivos, CancellationToken ct = default) =>
        await db.Categorias
            .Where(c => incluirInactivos || c.Activo)
            .OrderBy(c => c.Orden).ThenBy(c => c.Nombre)
            .Select(c => new CategoriaDto { Id = c.Id, Nombre = c.Nombre, Orden = c.Orden, Activo = c.Activo })
            .ToListAsync(ct);

    public async Task<Guid> CrearAsync(Guid usuarioId, GuardarCategoriaRequestDto request, CancellationToken ct = default)
    {
        var categoria = new Categoria
        {
            Nombre = request.Nombre,
            Orden = request.Orden,
            Activo = request.Activo,
            CreadoPorUsuarioId = usuarioId
        };
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync(ct);
        return categoria.Id;
    }

    public async Task ActualizarAsync(Guid categoriaId, GuardarCategoriaRequestDto request, CancellationToken ct = default)
    {
        var categoria = await db.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId, ct)
            ?? throw new InvalidOperationException("La categoría no existe.");

        categoria.Nombre = request.Nombre;
        categoria.Orden = request.Orden;
        categoria.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
    }
}
