using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Catalogo.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo;

// Catálogo global (no por sucursal) — Admin lo administra desde Central; el resto de
// la app solo lo lee para poblar el selector de unidad de medida en Productos.
public class UnidadMedidaAppService(IAppDbContext db)
{
    public async Task<List<UnidadMedidaDto>> ListarAsync(bool incluirInactivos, CancellationToken ct = default) =>
        await db.UnidadesMedida
            .Where(u => incluirInactivos || u.Activo)
            .OrderBy(u => u.Nombre)
            .Select(u => new UnidadMedidaDto { Id = u.Id, Nombre = u.Nombre, Activo = u.Activo })
            .ToListAsync(ct);

    public async Task<Guid> CrearAsync(GuardarUnidadMedidaRequestDto request, CancellationToken ct = default)
    {
        var yaExiste = await db.UnidadesMedida.AnyAsync(u => u.Nombre == request.Nombre, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe una unidad de medida llamada '{request.Nombre}'.");

        var unidad = new UnidadMedida { Nombre = request.Nombre, Activo = request.Activo };
        db.UnidadesMedida.Add(unidad);
        await db.SaveChangesAsync(ct);
        return unidad.Id;
    }

    public async Task ActualizarAsync(Guid unidadMedidaId, GuardarUnidadMedidaRequestDto request, CancellationToken ct = default)
    {
        var unidad = await db.UnidadesMedida.FirstOrDefaultAsync(u => u.Id == unidadMedidaId, ct)
            ?? throw new InvalidOperationException("La unidad de medida no existe.");

        var yaExiste = await db.UnidadesMedida.AnyAsync(u => u.Id != unidadMedidaId && u.Nombre == request.Nombre, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe otra unidad de medida llamada '{request.Nombre}'.");

        unidad.Nombre = request.Nombre;
        unidad.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
    }
}
