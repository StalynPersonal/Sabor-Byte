namespace SaborByte.Dominio.Pedidos;

public enum RolQueCancelo
{
    Mesero,
    Cocina,
    Supervisor
}

public class ComandaCancelacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ComandaId { get; set; }
    public Guid? ComandaItemId { get; set; } // null si se canceló la comanda completa
    public Guid CanceladoPorUsuarioId { get; set; }
    public RolQueCancelo RolQueCancelo { get; set; }
    public required string Motivo { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public bool InventarioRevertido { get; set; }
}
