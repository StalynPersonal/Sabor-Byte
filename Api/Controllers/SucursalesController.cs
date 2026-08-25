using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Sucursales;
using SaborByte.Aplicacion.Sucursales.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/sucursales")]
[Authorize]
public class SucursalesController(SucursalAppService sucursales) : ControllerBase
{
    // Admin: gestión global, no está atado a una sucursal en particular — es lo que
    // permite crear la primera sucursal y ver todas para dar de alta la siguiente.
    [HttpGet("gestion")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListarTodas(CancellationToken ct) => Ok(await sucursales.ListarTodasAsync(ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear(CrearSucursalRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await sucursales.CrearAsync(User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{sucursalId:guid}")]
    public async Task<IActionResult> Obtener(Guid sucursalId, CancellationToken ct)
    {
        // Admin gestiona cualquier sucursal desde "Sucursales" (no solo las suyas);
        // otros roles solo pueden leer la(s) sucursal(es) que tienen asignada(s).
        if (!User.IsInRole("Admin") && !User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            return Ok(await sucursales.ObtenerAsync(sucursalId, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{sucursalId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(Guid sucursalId, ActualizarSucursalRequestDto request, CancellationToken ct)
    {
        try
        {
            await sucursales.ActualizarAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{sucursalId:guid}/smtp")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActualizarSmtp(Guid sucursalId, ActualizarSmtpRequestDto request, CancellationToken ct)
    {
        try
        {
            await sucursales.ActualizarSmtpAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{sucursalId:guid}/smtp/prueba")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnviarPruebaSmtp(Guid sucursalId, EnviarPruebaSmtpRequestDto request, CancellationToken ct)
    {
        try
        {
            await sucursales.EnviarPruebaSmtpAsync(sucursalId, request.Destinatario, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            // Errores de SMTP (host incorrecto, credenciales inválidas, timeout, etc.) —
            // acá sí queremos que el mensaje real le llegue al Admin, a diferencia del
            // resto del sistema donde un fallo de correo se traga en silencio.
            return BadRequest(new { mensaje = $"No se pudo enviar: {ex.Message}" });
        }
    }
}
