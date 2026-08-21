using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.CxcCxp;
using SaborByte.Aplicacion.CxcCxp.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/cxccxp")]
[Authorize]
public class CxcCxpController(CxcCxpAppService cxcCxp) : ControllerBase
{
    [HttpGet("porcobrar")]
    public async Task<IActionResult> ListarPorCobrar([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await cxcCxp.ListarPorCobrarAsync(sucursalId, ct));
    }

    [HttpPost("porcobrar")]
    public async Task<IActionResult> CrearPorCobrar([FromQuery] Guid sucursalId, CrearCuentaPorCobrarRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        var id = await cxcCxp.CrearCuentaPorCobrarAsync(sucursalId, request, ct);
        return Ok(new { id });
    }

    [HttpPost("porcobrar/{cuentaId:guid}/pagos")]
    public async Task<IActionResult> PagarPorCobrar(Guid cuentaId, RegistrarPagoRequestDto request, CancellationToken ct)
    {
        try
        {
            await cxcCxp.RegistrarPagoCxCAsync(cuentaId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("porpagar")]
    public async Task<IActionResult> ListarPorPagar([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await cxcCxp.ListarPorPagarAsync(sucursalId, ct));
    }

    [HttpPost("porpagar")]
    public async Task<IActionResult> CrearPorPagar([FromQuery] Guid sucursalId, CrearCuentaPorPagarRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        var id = await cxcCxp.CrearCuentaPorPagarAsync(sucursalId, request, ct);
        return Ok(new { id });
    }

    [HttpPost("porpagar/{cuentaId:guid}/pagos")]
    public async Task<IActionResult> PagarPorPagar(Guid cuentaId, RegistrarPagoRequestDto request, CancellationToken ct)
    {
        try
        {
            await cxcCxp.RegistrarPagoCxPAsync(cuentaId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
