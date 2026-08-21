using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Reportes;
using SaborByte.Aplicacion.Reportes.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController(ReporteAppService reportes) : ControllerBase
{
    [HttpPost("ventas-por-sucursal")]
    public async Task<IActionResult> VentasPorSucursal(ReporteVentasRequestDto request, CancellationToken ct)
    {
        var sucursalesPermitidas = User.ObtenerSucursalesPermitidas();
        if (request.SucursalesIds.Except(sucursalesPermitidas).Any())
            return Forbid();

        return Ok(await reportes.VentasPorSucursalAsync(request, ct));
    }

    [HttpPost("ventas-por-producto")]
    public async Task<IActionResult> VentasPorProducto([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.VentasPorProductoAsync(sucursalId, request, ct));
    }

    [HttpPost("ventas-por-hora")]
    public async Task<IActionResult> VentasPorHora([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.VentasPorHoraAsync(sucursalId, request, ct));
    }
}
