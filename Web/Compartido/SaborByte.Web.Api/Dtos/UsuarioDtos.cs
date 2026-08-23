namespace SaborByte.Web.Api.Dtos;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Activo { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesAsignadas { get; set; } = [];
    public bool EsAdminPrincipal { get; set; }
}

public class CrearUsuarioRequestDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesAsignadas { get; set; } = [];
}

public class ActualizarUsuarioRequestDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Activo { get; set; } = true;
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesAsignadas { get; set; } = [];
}
