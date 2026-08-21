namespace SaborByte.Dominio.Identidad;

// Token de un solo uso emitido cuando un Supervisor/Admin autoriza una acción
// sensible (ej. descuento en una venta) solicitada por un cajero. Ver sección 7 del plan.
public class AutorizacionSupervisor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioAutorizanteId { get; set; }
    public required string Accion { get; set; } // ej. "Descuento"
    public DateTime Expira { get; set; } = DateTime.UtcNow.AddMinutes(3);
    public bool Usada { get; set; }
}
