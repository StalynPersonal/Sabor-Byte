namespace SaborByte.Aplicacion.Identidad.Dtos;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public required string NombreUsuario { get; set; }
    public required string Nombre { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesAsignadas { get; set; } = [];
}

public class CrearUsuarioRequestDto
{
    public required string NombreUsuario { get; set; }
    public required string Password { get; set; }
    public required string Nombre { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesAsignadas { get; set; } = [];
}
