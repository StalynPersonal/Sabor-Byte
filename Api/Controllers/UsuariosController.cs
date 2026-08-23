using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
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
    public async Task<IActionResult> Listar([FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct) =>
        Ok(await usuarios.ListarAsync(pagina == 0 ? 1 : pagina, tamanoPagina == 0 ? 20 : tamanoPagina, ct));

    [HttpPost]
    public async Task<IActionResult> Crear(CrearUsuarioRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await usuarios.CrearAsync(User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{usuarioId:guid}")]
    public async Task<IActionResult> Actualizar(Guid usuarioId, ActualizarUsuarioRequestDto request, CancellationToken ct)
    {
        try
        {
            await usuarios.ActualizarAsync(usuarioId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
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
            await usuarios.DesactivarAsync(usuarioId, User.ObtenerUsuarioId(), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
