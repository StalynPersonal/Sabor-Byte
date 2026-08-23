using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Catalogo;
using SaborByte.Aplicacion.Catalogo.Dtos;

namespace SaborByte.Api.Controllers;

// Catálogo de toda la empresa, igual que Productos — cualquier usuario autenticado
// puede consultarlo; solo Admin lo modifica.
[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController(CategoriaAppService categorias) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos, CancellationToken ct) =>
        Ok(await categorias.ListarAsync(incluirInactivos, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear(GuardarCategoriaRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await categorias.CrearAsync(User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{categoriaId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(Guid categoriaId, GuardarCategoriaRequestDto request, CancellationToken ct)
    {
        try
        {
            await categorias.ActualizarAsync(categoriaId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
