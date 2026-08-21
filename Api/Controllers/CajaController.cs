using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Caja;
using SaborByte.Aplicacion.Caja.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/caja")]
[Authorize]
public class CajaController(CajaAppService cajaAppService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await cajaAppService.ListarCajasAsync(sucursalId, ct));
    }

    [HttpPost("turnos/abrir")]
    public async Task<IActionResult> AbrirTurno(AbrirTurnoRequestDto request, CancellationToken ct)
    {
        // La IP de origen se toma del propio request (no del cliente) para que no pueda falsearse.
        request.IpOrigen = HttpContext.Connection.RemoteIpAddress?.ToString() ?? request.IpOrigen;

        try
        {
            var turnoId = await cajaAppService.AbrirTurnoAsync(User.ObtenerUsuarioId(), request, ct);
            return Ok(new { turnoCajaId = turnoId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("turnos/{turnoCajaId:guid}/resumen")]
    public async Task<ActionResult<ResumenTurnoDto>> ObtenerResumen(Guid turnoCajaId, CancellationToken ct)
    {
        try
        {
            return Ok(await cajaAppService.ObtenerResumenAsync(turnoCajaId, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost("turnos/cerrar")]
    public async Task<IActionResult> CerrarTurno(CerrarTurnoRequestDto request, CancellationToken ct)
    {
        try
        {
            await cajaAppService.CerrarTurnoAsync(User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
