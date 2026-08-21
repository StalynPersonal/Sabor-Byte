using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Catalogo;
using SaborByte.Aplicacion.Catalogo.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController(CategoriaAppService categorias) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await categorias.ListarAsync(sucursalId, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, GuardarCategoriaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        var id = await categorias.CrearAsync(sucursalId, request, ct);
        return Ok(new { id });
    }

    [HttpPut("{categoriaId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar([FromQuery] Guid sucursalId, Guid categoriaId, GuardarCategoriaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            await categorias.ActualizarAsync(sucursalId, categoriaId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
