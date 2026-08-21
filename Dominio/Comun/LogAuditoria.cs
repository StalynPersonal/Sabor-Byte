namespace SaborByte.Dominio.Comun;

public class LogAuditoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SucursalId { get; set; }
    public Guid UsuarioId { get; set; }
    public required string Accion { get; set; } // ej. "Descuento", "CancelacionItem", "CierreCaja", "CambioConfiguracion"
    public required string Entidad { get; set; } // ej. "Factura", "ComandaItem", "TurnoCaja", "Sucursal"
    public Guid? EntidadId { get; set; }
    public string? Detalle { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}
