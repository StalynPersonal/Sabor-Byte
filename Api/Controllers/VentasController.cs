using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Facturacion;
using SaborByte.Aplicacion.Facturacion.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize]
public class VentasController(VentaAppService ventas) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<VentaResultadoDto>> Crear(
        [FromQuery] Guid sucursalId, CrearVentaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            var resultado = await ventas.CrearVentaAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
