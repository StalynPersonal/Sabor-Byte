namespace SaborByte.Aplicacion.Identidad.Dtos;

// Identifica desde cuál de las 4 apps llega el login — necesario para poder filtrar, por
// ejemplo, las sucursales donde el módulo Mesero/Cocina esté deshabilitado (ver
// AutenticacionAppService.LoginAsync). Caja y Central no tienen bandera de módulo propia,
// así que no se filtran.
public enum AppCliente
{
    Central,
    Caja,
    Cocina,
    Mesero
}

public class LoginRequestDto
{
    public required string NombreUsuario { get; set; }
    public required string Password { get; set; }
    public AppCliente App { get; set; }
}

public class SucursalPermitidaDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }

    // Nombre de la Empresa dueña de esta sucursal — el que se muestra en el AppBar/login
    // de las 4 apps una vez que el usuario elige/tiene activa esta sucursal.
    public string? EmpresaNombre { get; set; }
}

public class LoginResponseDto
{
    public required string Token { get; set; }
    public Guid UsuarioId { get; set; }
    public required string Nombre { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<SucursalPermitidaDto> SucursalesPermitidas { get; set; } = [];
}

public class SeleccionarSucursalActivaRequestDto
{
    public Guid SucursalId { get; set; }
}

public class SeleccionarCajaActivaRequestDto
{
    public Guid CajaId { get; set; }
}

public class SesionActivaDto
{
    public Guid UsuarioId { get; set; }
    public required string NombreUsuario { get; set; }
    public required string Nombre { get; set; }
    public Guid? SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaUltimaActividad { get; set; }
}
