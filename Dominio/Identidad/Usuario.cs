namespace SaborByte.Dominio.Identidad;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string NombreUsuario { get; set; }
    public required string HashPassword { get; set; }
    public required string Nombre { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<UsuarioRol> Roles { get; set; } = [];
    public ICollection<UsuarioSucursal> SucursalesAsignadas { get; set; } = [];
}
