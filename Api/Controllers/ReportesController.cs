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
}
