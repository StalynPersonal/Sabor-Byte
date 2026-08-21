namespace SaborByte.Aplicacion.Interfaces;

public interface IAuditoriaService
{
    Task RegistrarAsync(
        Guid? sucursalId, Guid usuarioId, string accion, string entidad,
        Guid? entidadId = null, string? detalle = null, CancellationToken ct = default);
}
