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
    [HttpGet("{sucursalId:guid}")]
    public async Task<IActionResult> Obtener(Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
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
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

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
}
