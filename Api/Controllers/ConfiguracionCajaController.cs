using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Aplicacion.Caja;
using SaborByte.Aplicacion.Caja.Dtos;

namespace SaborByte.Api.Controllers;

// Catálogos globales — cualquier usuario autenticado puede listarlos (Caja los necesita
// para el selector de propina y el desglose de efectivo del cierre); solo Admin crea/edita.
[ApiController]
[Route("api/configuracioncaja")]
[Authorize]
public class ConfiguracionCajaController(ConfiguracionCajaAppService configuracion) : ControllerBase
{
    [HttpGet("propinas")]
    public async Task<IActionResult> ListarPorcentajesPropina([FromQuery] bool incluirInactivos, CancellationToken ct) =>
        Ok(await configuracion.ListarPorcentajesPropinaAsync(incluirInactivos, ct));

    [HttpPost("propinas")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CrearPorcentajePropina(GuardarPorcentajePropinaRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await configuracion.CrearPorcentajePropinaAsync(request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("propinas/{porcentajeId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActualizarPorcentajePropina(Guid porcentajeId, GuardarPorcentajePropinaRequestDto request, CancellationToken ct)
    {
        try
        {
            await configuracion.ActualizarPorcentajePropinaAsync(porcentajeId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("denominaciones")]
    public async Task<IActionResult> ListarDenominacionesEfectivo([FromQuery] bool incluirInactivos, CancellationToken ct) =>
        Ok(await configuracion.ListarDenominacionesEfectivoAsync(incluirInactivos, ct));

    [HttpPost("denominaciones")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CrearDenominacionEfectivo(GuardarDenominacionEfectivoRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await configuracion.CrearDenominacionEfectivoAsync(request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("denominaciones/{denominacionId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActualizarDenominacionEfectivo(Guid denominacionId, GuardarDenominacionEfectivoRequestDto request, CancellationToken ct)
    {
        try
        {
            await configuracion.ActualizarDenominacionEfectivoAsync(denominacionId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
