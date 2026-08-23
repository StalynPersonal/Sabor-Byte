using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Facturacion.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Facturacion;

namespace SaborByte.Aplicacion.Facturacion;

// Catálogo global de motivos de nota de crédito/débito — Admin lo administra desde
// Central; el resto de la app solo lo lee para poblar el select al emitir una nota.
public class MotivoNotaCreditoAppService(IAppDbContext db)
{
    public async Task<List<MotivoNotaCreditoDto>> ListarAsync(bool incluirInactivos, CancellationToken ct = default) =>
        await db.MotivosNotaCredito
            .Where(m => incluirInactivos || m.Activo)
            .OrderBy(m => m.Nombre)
            .Select(m => new MotivoNotaCreditoDto { Id = m.Id, Nombre = m.Nombre, Activo = m.Activo })
            .ToListAsync(ct);

    public async Task<Guid> CrearAsync(GuardarMotivoNotaCreditoRequestDto request, CancellationToken ct = default)
    {
        var yaExiste = await db.MotivosNotaCredito.AnyAsync(m => m.Nombre == request.Nombre, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe un motivo llamado '{request.Nombre}'.");

        var motivo = new MotivoNotaCredito { Nombre = request.Nombre, Activo = request.Activo };
        db.MotivosNotaCredito.Add(motivo);
        await db.SaveChangesAsync(ct);
        return motivo.Id;
    }

    public async Task ActualizarAsync(Guid motivoId, GuardarMotivoNotaCreditoRequestDto request, CancellationToken ct = default)
    {
        var motivo = await db.MotivosNotaCredito.FirstOrDefaultAsync(m => m.Id == motivoId, ct)
            ?? throw new InvalidOperationException("El motivo no existe.");

        var yaExiste = await db.MotivosNotaCredito.AnyAsync(m => m.Id != motivoId && m.Nombre == request.Nombre, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe otro motivo llamado '{request.Nombre}'.");

        motivo.Nombre = request.Nombre;
        motivo.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
    }
}
