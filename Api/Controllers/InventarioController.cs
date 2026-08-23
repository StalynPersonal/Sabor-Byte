using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Inventario;
using SaborByte.Aplicacion.Inventario.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/inventario")]
[Authorize]
public class InventarioController(InventarioAppService inventario) : ControllerBase
{
    [HttpGet("movimientos")]
    public async Task<IActionResult> ListarMovimientos(
        [FromQuery] Guid sucursalId, [FromQuery] Guid? productoId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await inventario.ListarMovimientosAsync(sucursalId, productoId, ct));
    }

    [HttpPost("entradas")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> RegistrarEntrada(
        [FromQuery] Guid sucursalId, RegistrarEntradaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            var id = await inventario.RegistrarEntradaAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("ajustes")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> RegistrarAjuste(
        [FromQuery] Guid sucursalId, RegistrarAjusteRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            var id = await inventario.RegistrarAjusteAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // Configura el Stock Mínimo/Máximo (umbral de alerta) de un insumo para UNA
    // sucursal puntual — el stock, y por lo tanto sus umbrales, son por sucursal.
    [HttpPut("umbrales")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> ConfigurarUmbrales(
        [FromQuery] Guid sucursalId, ConfigurarUmbralesRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            await inventario.ConfigurarUmbralesAsync(sucursalId, request.ProductoId, request.StockMinimo, request.StockMaximo, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
