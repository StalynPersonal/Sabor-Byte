using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Pedidos;
using SaborByte.Aplicacion.Pedidos.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/comandas")]
[Authorize]
public class ComandasController(ComandaAppService comandas) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerAbiertas([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await comandas.ObtenerAbiertasAsync(sucursalId, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, CrearComandaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            return Ok(await comandas.CrearComandaAsync(sucursalId, User.ObtenerUsuarioId(), request, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("items/{comandaItemId:guid}/estado")]
    public async Task<IActionResult> CambiarEstadoItem(
        [FromQuery] Guid sucursalId, Guid comandaItemId, CambiarEstadoItemRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            return Ok(await comandas.CambiarEstadoItemAsync(sucursalId, comandaItemId, request.NuevoEstado, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("items/{comandaItemId:guid}/cancelar")]
    public async Task<IActionResult> CancelarItem(
        [FromQuery] Guid sucursalId, Guid comandaItemId, CancelarItemRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            await comandas.CancelarItemAsync(sucursalId, comandaItemId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
