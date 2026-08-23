using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Aplicacion.Catalogo;
using SaborByte.Aplicacion.Catalogo.Dtos;

namespace SaborByte.Api.Controllers;

// Catálogo global — cualquier usuario autenticado puede listarlo (para los selectores
// de forma de pago en Caja y CxC/CxP); solo Admin puede crear/editar.
[ApiController]
[Route("api/metodospago")]
[Authorize]
public class MetodosPagoController(MetodoPagoAppService metodosPago) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos, CancellationToken ct) =>
        Ok(await metodosPago.ListarAsync(incluirInactivos, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear(GuardarMetodoPagoRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await metodosPago.CrearAsync(request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{metodoPagoId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(Guid metodoPagoId, GuardarMetodoPagoRequestDto request, CancellationToken ct)
    {
        try
        {
            await metodosPago.ActualizarAsync(metodoPagoId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
