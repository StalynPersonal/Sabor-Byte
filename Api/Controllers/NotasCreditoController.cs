using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Facturacion;
using SaborByte.Aplicacion.Facturacion.Dtos;

namespace SaborByte.Api.Controllers;

// Solo lectura desde Central (listar/ver detalle); la emisión ("Crear") solo la usa
// Caja, gatillada por autorización de Supervisor/Admin — ver NotaCreditoAppService.CrearAsync.
[ApiController]
[Route("api/notascredito")]
[Authorize]
public class NotasCreditoController(NotaCreditoAppService notas) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Cajero,Supervisor,Admin")]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, CrearNotaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            return Ok(await notas.CrearAsync(sucursalId, User.ObtenerUsuarioId(), request, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("facturas/{facturaId:guid}/detalle")]
    public async Task<IActionResult> ObtenerDetalleDisponible([FromQuery] Guid sucursalId, Guid facturaId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            return Ok(await notas.ObtenerDetalleDisponibleAsync(sucursalId, facturaId, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet("por-factura/{facturaId:guid}")]
    public async Task<IActionResult> ListarPorFactura([FromQuery] Guid sucursalId, Guid facturaId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await notas.ListarPorFacturaAsync(sucursalId, facturaId, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid sucursalId, [FromQuery] string? texto,
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] decimal? montoMinimo, [FromQuery] decimal? montoMaximo, [FromQuery] Guid? cajaId,
        [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await notas.ListarAsync(
            sucursalId, texto, desde, hasta, montoMinimo, montoMaximo, cajaId,
            pagina == 0 ? 1 : pagina, tamanoPagina == 0 ? 20 : tamanoPagina, ct));
    }
}
