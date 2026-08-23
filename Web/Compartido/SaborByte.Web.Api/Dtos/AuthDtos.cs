namespace SaborByte.Web.Api.Dtos;

public class LoginRequestDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SucursalPermitidaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? EmpresaNombre { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public List<SucursalPermitidaDto> SucursalesPermitidas { get; set; } = [];
}

public class SeleccionarSucursalActivaRequestDto
{
    public Guid SucursalId { get; set; }
}

public class CambiarPasswordRequestDto
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}

public class SesionActivaDto
{
    public Guid UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Guid? SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaUltimaActividad { get; set; }
}
