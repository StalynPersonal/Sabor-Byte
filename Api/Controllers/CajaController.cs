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

    // --- CRUD de cajas (Admin): gestión de las cajas de la sucursal, incluyendo el
    // consecutivo (ProximoNumeroFactura) usado para el número de factura interno. ---

    // Admin gestiona las cajas de CUALQUIER sucursal desde Central (no solo la suya) —
    // por eso estos tres endpoints (a diferencia del GET simple de arriba) no exigen que
    // el usuario esté asignado a esa sucursal, solo que tenga el rol Admin.

    [HttpGet("gestion")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListarTodas([FromQuery] Guid sucursalId, CancellationToken ct) =>
        Ok(await cajaAppService.ListarTodasAsync(sucursalId, ct));

    [HttpPost("gestion")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, GuardarCajaRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await cajaAppService.CrearCajaAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("gestion/{cajaId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar([FromQuery] Guid sucursalId, Guid cajaId, GuardarCajaRequestDto request, CancellationToken ct)
    {

        try
        {
            await cajaAppService.ActualizarCajaAsync(sucursalId, cajaId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("turnos/abierto")]
    public async Task<IActionResult> ObtenerTurnoAbierto([FromQuery] Guid cajaId, CancellationToken ct)
    {
        if (cajaId == Guid.Empty)
            return Ok(null);

        return Ok(await cajaAppService.ObtenerTurnoAbiertoAsync(cajaId, User.ObtenerSucursalesPermitidas(), ct));
    }

    [HttpPost("turnos/abrir")]
    public async Task<IActionResult> AbrirTurno(AbrirTurnoRequestDto request, CancellationToken ct)
    {
        // La IP de origen se toma del propio request (no del cliente) para que no pueda falsearse.
        request.IpOrigen = HttpContext.Connection.RemoteIpAddress?.ToString() ?? request.IpOrigen;

        try
        {
            var turnoId = await cajaAppService.AbrirTurnoAsync(
                User.ObtenerUsuarioId(), User.ObtenerSucursalesPermitidas(), request, ct);
            return Ok(new { turnoCajaId = turnoId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // Historial de turnos ya cerrados (el "cuadre" de caja) — para la pantalla "Cuadres
    // de Caja" en Central. No exige que el usuario esté asignado a la sucursal si es
    // Admin, mismo criterio que "gestion" en otros controllers.
    [HttpGet("turnos/cerrados")]
    public async Task<IActionResult> ListarTurnosCerrados(
        [FromQuery] Guid sucursalId, [FromQuery] Guid? cajaId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.IsInRole("Admin") && !User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await cajaAppService.ListarTurnosCerradosAsync(
            sucursalId, cajaId, desde, hasta, pagina == 0 ? 1 : pagina, tamanoPagina == 0 ? 20 : tamanoPagina, ct));
    }

    // Detalle completo de un cuadre (abierto o cerrado) — reutiliza ObtenerResumenAsync,
    // que ya trae esperado/contado/diferencia y el desglose de denominaciones cuando el
    // turno está cerrado.
    [HttpGet("turnos/{turnoCajaId:guid}/detalle")]
    public async Task<IActionResult> ObtenerDetalleCuadre(Guid turnoCajaId, CancellationToken ct)
    {
        try
        {
            return Ok(await cajaAppService.ObtenerResumenAsync(turnoCajaId, User.ObtenerSucursalesPermitidas(), ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet("turnos/{turnoCajaId:guid}/resumen")]
    public async Task<ActionResult<ResumenTurnoDto>> ObtenerResumen(Guid turnoCajaId, CancellationToken ct)
    {
        try
        {
            return Ok(await cajaAppService.ObtenerResumenAsync(turnoCajaId, User.ObtenerSucursalesPermitidas(), ct));
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
            await cajaAppService.CerrarTurnoAsync(User.ObtenerUsuarioId(), User.ObtenerSucursalesPermitidas(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
