using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Identidad;
using SaborByte.Aplicacion.Identidad.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AutenticacionAppService autenticacion, UsuarioAppService usuarios) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request, CancellationToken ct)
    {
        var ipOrigen = HttpContext.Connection.RemoteIpAddress?.ToString();
        var resultado = await autenticacion.LoginAsync(request, ipOrigen, ct);
        if (resultado is null)
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

        return Ok(resultado);
    }

    [HttpPost("sesion/sucursal")]
    [Authorize]
    public async Task<IActionResult> SeleccionarSucursalActiva(SeleccionarSucursalActivaRequestDto request, CancellationToken ct)
    {
        try
        {
            await autenticacion.SeleccionarSucursalActivaAsync(
                User.ObtenerSesionActivaId(), User.ObtenerUsuarioId(), request.SucursalId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // Autoservicio: cualquier usuario autenticado (no solo Admin) puede cambiar su propia
    // contraseña — por eso vive en AuthController y no en UsuariosController (que es
    // Admin-only a nivel de clase).
    [HttpPost("cambiar-password")]
    [Authorize]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordRequestDto request, CancellationToken ct)
    {
        try
        {
            await usuarios.CambiarPasswordAsync(User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await autenticacion.CerrarSesionAsync(User.ObtenerSesionActivaId(), User.ObtenerUsuarioId(), ct);
        return NoContent();
    }

    [HttpGet("sesiones-activas")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> ListarSesionesActivas(CancellationToken ct) =>
        Ok(await autenticacion.ListarActivasAsync(User.ObtenerSucursalesPermitidas(), ct));
}
