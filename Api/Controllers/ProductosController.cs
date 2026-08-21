using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Catalogo;
using SaborByte.Aplicacion.Catalogo.Dtos;

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

    [HttpGet("todos")]
    public async Task<IActionResult> Listar([FromQuery] Guid sucursalId, [FromQuery] bool incluirInactivos, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await productos.ListarAsync(sucursalId, incluirInactivos, ct));
    }

    [HttpGet("{productoId:guid}")]
    public async Task<IActionResult> Obtener(Guid productoId, CancellationToken ct)
    {
        try
        {
            return Ok(await productos.ObtenerAsync(productoId, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, GuardarProductoRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        var id = await productos.CrearAsync(sucursalId, request, ct);
        return Ok(new { id });
    }

    [HttpPut("{productoId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(Guid productoId, GuardarProductoRequestDto request, CancellationToken ct)
    {
        try
        {
            await productos.ActualizarAsync(productoId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{productoId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Desactivar(Guid productoId, CancellationToken ct)
    {
        try
        {
            await productos.DesactivarAsync(productoId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost("combos")]
    [Authorize(Roles = "Supervisor,Admin")]
    public async Task<IActionResult> CrearCombo([FromQuery] Guid sucursalId, CrearComboRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            var id = await productos.CrearComboAsync(sucursalId, request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
