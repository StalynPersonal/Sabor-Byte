using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Facturacion;

namespace SaborByte.Api.Controllers;

// Solo lectura: las facturas SIEMPRE se generan desde Caja (api/ventas); aquí solo se
// consultan (listado + detalle completo para el ícono de "ver" en Central).
[ApiController]
[Route("api/facturas")]
[Authorize]
public class FacturasController(FacturaAppService facturas) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Buscar(
        [FromQuery] Guid sucursalId, [FromQuery] string? texto,
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] decimal? montoMinimo, [FromQuery] decimal? montoMaximo, [FromQuery] Guid? cajaId,
        [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await facturas.BuscarAsync(
            sucursalId, texto, desde, hasta, montoMinimo, montoMaximo, cajaId,
            pagina == 0 ? 1 : pagina, tamanoPagina == 0 ? 20 : tamanoPagina, ct));
    }

    [HttpGet("{facturaId:guid}")]
    public async Task<IActionResult> ObtenerDetalle([FromQuery] Guid sucursalId, Guid facturaId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            return Ok(await facturas.ObtenerDetalleCompletoAsync(sucursalId, facturaId, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
