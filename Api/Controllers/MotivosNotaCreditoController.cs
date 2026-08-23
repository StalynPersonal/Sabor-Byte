using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Aplicacion.Facturacion;
using SaborByte.Aplicacion.Facturacion.Dtos;

namespace SaborByte.Api.Controllers;

// Catálogo global (no por sucursal): cualquier usuario autenticado puede listarlo para
// poblar el select al emitir una nota; solo Admin puede administrarlo.
[ApiController]
[Route("api/motivosnotacredito")]
[Authorize]
public class MotivosNotaCreditoController(MotivoNotaCreditoAppService motivos) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos, CancellationToken ct) =>
        Ok(await motivos.ListarAsync(incluirInactivos, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear(GuardarMotivoNotaCreditoRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await motivos.CrearAsync(request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{motivoId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(Guid motivoId, GuardarMotivoNotaCreditoRequestDto request, CancellationToken ct)
    {
        try
        {
            await motivos.ActualizarAsync(motivoId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
