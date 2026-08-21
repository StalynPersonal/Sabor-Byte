using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Catalogo;

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
}
