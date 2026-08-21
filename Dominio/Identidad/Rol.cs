namespace SaborByte.Dominio.Identidad;

public enum RolBase
{
    Admin,
    Supervisor,
    Cajero,
    Mesero,
    Cocina
}

public class Rol
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public ICollection<RolPermiso> Permisos { get; set; } = [];
}
