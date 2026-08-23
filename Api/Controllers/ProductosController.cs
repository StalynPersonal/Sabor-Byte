using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Catalogo;
using SaborByte.Aplicacion.Catalogo.Dtos;

namespace SaborByte.Api.Controllers;

// Catálogo de toda la empresa (no por sucursal, ver comentario de clase en Producto.cs) —
// cualquier usuario autenticado puede consultarlo; solo Admin/Supervisor lo modifican.
// sucursalId es opcional en las lecturas: si se manda, además de la existencia del
// producto trae/valida el STOCK de esa sucursal puntual (que sí es por sucursal).
[ApiController]
[Route("api/productos")]
[Authorize]
public class ProductosController(ProductoAppService productos) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] string? texto, [FromQuery] Guid? categoriaId, CancellationToken ct) =>
        Ok(await productos.BuscarAsync(texto ?? string.Empty, categoriaId, ct));

    [HttpGet("todos")]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina, [FromQuery] int tamanoPagina,
        [FromQuery] string? texto, [FromQuery] Dominio.Catalogo.TipoProducto? tipo,
        [FromQuery] bool incluirInactivos, [FromQuery] Guid? sucursalId, CancellationToken ct)
    {
        if (sucursalId is Guid sid && !User.IsInRole("Admin") && !User.TieneAccesoASucursal(sid))
            return Forbid();

        return Ok(await productos.ListarAsync(
            pagina == 0 ? 1 : pagina, tamanoPagina == 0 ? 20 : tamanoPagina,
            texto, tipo, incluirInactivos, sucursalId, ct));
    }

    [HttpGet("{productoId:guid}")]
    public async Task<IActionResult> Obtener(Guid productoId, [FromQuery] Guid? sucursalId, CancellationToken ct)
    {
        if (sucursalId is Guid sid && !User.IsInRole("Admin") && !User.TieneAccesoASucursal(sid))
            return Forbid();

        try
        {
            return Ok(await productos.ObtenerAsync(productoId, sucursalId, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear(GuardarProductoRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await productos.CrearAsync(User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{productoId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(Guid productoId, GuardarProductoRequestDto request, CancellationToken ct)
    {
        try
        {
            await productos.ActualizarAsync(productoId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
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

    [HttpPost("{productoId:guid}/activar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activar(Guid productoId, CancellationToken ct)
    {
        try
        {
            await productos.ActivarAsync(productoId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost("combos")]
    [Authorize(Roles = "Supervisor,Admin")]
    public async Task<IActionResult> CrearCombo(CrearComboRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await productos.CrearComboAsync(User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
