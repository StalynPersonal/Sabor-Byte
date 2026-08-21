namespace SaborByte.Aplicacion.Identidad.Dtos;

public class LoginRequestDto
{
    public required string NombreUsuario { get; set; }
    public required string Password { get; set; }
}

public class LoginResponseDto
{
    public required string Token { get; set; }
    public required string Nombre { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<Guid> SucursalesPermitidas { get; set; } = [];
}
