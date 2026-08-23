namespace SaborByte.Dominio.Identidad;

// Registro de presencia: quién inició sesión, en qué sucursal está operando y cuándo
// fue su última actividad — el login por JWT es sin estado, así que sin esta tabla no
// hay forma de saber "quién está activo ahora mismo y dónde" (necesario para el
// dashboard de Central y para operar con criterio en multisucursal).
public class SesionActiva
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // Null hasta que el usuario elige sucursal (si tiene más de una asignada).
    public Guid? SucursalId { get; set; }

    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime FechaUltimaActividad { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }
    public string? IpOrigen { get; set; }
}
