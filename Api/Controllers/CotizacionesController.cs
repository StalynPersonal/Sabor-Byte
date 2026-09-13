using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Cotizaciones;
using SaborByte.Aplicacion.Cotizaciones.Dtos;
using SaborByte.Dominio.Cotizaciones;

namespace SaborByte.Api.Controllers;

// Se crean/editan igual desde Central o desde Caja — cualquier usuario autenticado con
// acceso a la sucursal puede administrarlas, mismo criterio que Clientes.
[ApiController]
[Route("api/cotizaciones")]
[Authorize]
public class CotizacionesController(CotizacionAppService cotizaciones) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid sucursalId, [FromQuery] string? texto, [FromQuery] EstadoCotizacion? estado,
        [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        return Ok(await cotizaciones.ListarAsync(
            sucursalId, texto, estado, pagina == 0 ? 1 : pagina, tamanoPagina == 0 ? 20 : tamanoPagina, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener([FromQuery] Guid sucursalId, Guid id, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            return Ok(await cotizaciones.ObtenerAsync(sucursalId, id, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, GuardarCotizacionRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            var id = await cotizaciones.CrearAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar([FromQuery] Guid sucursalId, Guid id, GuardarCotizacionRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await cotizaciones.ActualizarAsync(sucursalId, id, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado([FromQuery] Guid sucursalId, Guid id, [FromQuery] EstadoCotizacion nuevoEstado, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await cotizaciones.CambiarEstadoAsync(sucursalId, id, nuevoEstado, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar([FromQuery] Guid sucursalId, Guid id, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await cotizaciones.EliminarAsync(sucursalId, id, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cargar-carrito")]
    public async Task<IActionResult> CargarEnCarrito([FromQuery] Guid sucursalId, Guid id, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            return Ok(await cotizaciones.CargarEnCarritoAsync(sucursalId, id, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:guid}/recrear")]
    public async Task<IActionResult> Recrear([FromQuery] Guid sucursalId, Guid id, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            var nuevoId = await cotizaciones.RecrearAsync(sucursalId, id, User.ObtenerUsuarioId(), ct);
            return Ok(new { id = nuevoId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
