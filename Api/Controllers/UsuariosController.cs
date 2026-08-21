using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Aplicacion.Identidad;
using SaborByte.Aplicacion.Identidad.Dtos;

namespace SaborByte.Api.Controllers;

// Solo Admin gestiona usuarios y su asignación a sucursales (ver sección 7 del plan).
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Admin")]
public class UsuariosController(UsuarioAppService usuarios) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct) => Ok(await usuarios.ListarAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Crear(CrearUsuarioRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await usuarios.CrearAsync(request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{usuarioId:guid}")]
    public async Task<IActionResult> Desactivar(Guid usuarioId, CancellationToken ct)
    {
        try
        {
            await usuarios.DesactivarAsync(usuarioId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
