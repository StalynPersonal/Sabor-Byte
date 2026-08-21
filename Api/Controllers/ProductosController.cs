using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Catalogo;
using SaborByte.Aplicacion.Catalogo.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize]
public class ProductosController(ProductoAppService productos) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] Guid sucursalId, [FromQuery] string? texto, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        var resultado = await productos.BuscarAsync(sucursalId, texto ?? string.Empty, ct);
        return Ok(resultado);
    }

    [HttpPost("combos")]
    [Authorize(Roles = "Supervisor,Admin")]
    public async Task<IActionResult> CrearCombo([FromQuery] Guid sucursalId, CrearComboRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            var id = await productos.CrearComboAsync(sucursalId, request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
