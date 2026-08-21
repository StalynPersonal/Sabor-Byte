using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Comun;
using SaborByte.Infraestructura.Persistencia;

namespace SaborByte.Infraestructura.Auditoria;

public class AuditoriaService(SaborByteDbContext db) : IAuditoriaService
{
    public async Task RegistrarAsync(
        Guid? sucursalId, Guid usuarioId, string accion, string entidad,
        Guid? entidadId = null, string? detalle = null, CancellationToken ct = default)
    {
        db.LogsAuditoria.Add(new LogAuditoria
        {
            SucursalId = sucursalId,
            UsuarioId = usuarioId,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Detalle = detalle
        });

        await db.SaveChangesAsync(ct);
    }
}
