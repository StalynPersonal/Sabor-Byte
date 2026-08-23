using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Aplicacion.Catalogo;
using SaborByte.Aplicacion.Catalogo.Dtos;

namespace SaborByte.Api.Controllers;

// Catálogo global — cualquier usuario autenticado puede listarlo (para el selector de
// unidad de medida en Productos); solo Admin puede crear/editar.
[ApiController]
[Route("api/unidadesmedida")]
[Authorize]
public class UnidadesMedidaController(UnidadMedidaAppService unidadesMedida) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos, CancellationToken ct) =>
        Ok(await unidadesMedida.ListarAsync(incluirInactivos, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear(GuardarUnidadMedidaRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await unidadesMedida.CrearAsync(request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{unidadMedidaId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(Guid unidadMedidaId, GuardarUnidadMedidaRequestDto request, CancellationToken ct)
    {
        try
        {
            await unidadesMedida.ActualizarAsync(unidadMedidaId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
