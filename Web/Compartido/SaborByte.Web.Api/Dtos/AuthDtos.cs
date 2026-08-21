namespace SaborByte.Web.Api.Dtos;

public class LoginRequestDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesPermitidas { get; set; } = [];
}
