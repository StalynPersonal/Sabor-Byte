using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaborByte.Aplicacion.Identidad;
using SaborByte.Aplicacion.Identidad.Dtos;

namespace SaborByte.Api.Controllers;

// Flujo de "supervisor override" (sección 7 del plan): el cajero llama a este
// endpoint con las credenciales de un Supervisor/Admin y recibe un código de un
// solo uso para adjuntar a la operación sensible (ej. descuento en una venta).
[ApiController]
[Route("api/autorizaciones")]
[Authorize]
public class AutorizacionesController(AutorizacionAppService autorizacion) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Solicitar(SolicitarAutorizacionRequestDto request, CancellationToken ct)
    {
        try
        {
            return Ok(await autorizacion.SolicitarAsync(request, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
