namespace SaborByte.Aplicacion.Identidad.Dtos;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public required string NombreUsuario { get; set; }
    public required string Nombre { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
    public bool RecibirAlertaStockBajo { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesAsignadas { get; set; } = [];

    // El primer usuario creado en el sistema — no se puede editar ni desactivar (ver
    // UsuarioAppService.ObtenerIdAdminPrincipalAsync). El frontend usa esto para
    // deshabilitar sus botones de editar/desactivar.
    public bool EsAdminPrincipal { get; set; }
}

public class CambiarPasswordRequestDto
{
    public required string PasswordActual { get; set; }
    public required string PasswordNueva { get; set; }
}

public class CrearUsuarioRequestDto
{
    public required string NombreUsuario { get; set; }
    public required string Password { get; set; }
    public required string Nombre { get; set; }
    public string? Email { get; set; }
    public bool RecibirAlertaStockBajo { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesAsignadas { get; set; } = [];
}

public class ActualizarUsuarioRequestDto
{
    public required string NombreUsuario { get; set; }
    // Vacío/null = no se cambia la contraseña actual.
    public string? Password { get; set; }
    public required string Nombre { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; } = true;
    public bool RecibirAlertaStockBajo { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesAsignadas { get; set; } = [];
}
