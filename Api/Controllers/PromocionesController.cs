using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Aplicacion.Catalogo;
using SaborByte.Aplicacion.Catalogo.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/promociones")]
[Authorize]
public class PromocionesController(PromocionAppService promociones) : ControllerBase
{
    // Gestión completa (Central) — solo Admin.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await promociones.ListarAsync(ct));

    // Consulta liviana (Caja) para aplicar descuento automático al facturar.
    [HttpGet("vigentes")]
    public async Task<IActionResult> ListarVigentes([FromQuery] Guid sucursalId, CancellationToken ct)
        => Ok(await promociones.ListarVigentesAsync(sucursalId, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear(GuardarPromocionRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await promociones.CrearAsync(request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{promocionId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(Guid promocionId, GuardarPromocionRequestDto request, CancellationToken ct)
    {
        try
        {
            await promociones.ActualizarAsync(promocionId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
