namespace SaborByte.Dominio.Comun;

public abstract class EntidadBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public Guid? CreadoPorUsuarioId { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public Guid? ActualizadoPorUsuarioId { get; set; }
}
