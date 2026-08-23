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
    [HttpGet("proveedores")]
    public async Task<IActionResult> ListarProveedores([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await cxcCxp.ListarProveedoresAsync(sucursalId, ct));
    }

    [HttpPost("proveedores")]
    public async Task<IActionResult> CrearProveedor([FromQuery] Guid sucursalId, GuardarProveedorRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            var id = await cxcCxp.CrearProveedorAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("porcobrar")]
    public async Task<IActionResult> ListarPorCobrar([FromQuery] Guid sucursalId, [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await cxcCxp.ListarPorCobrarAsync(sucursalId, pagina == 0 ? 1 : pagina, tamanoPagina == 0 ? 20 : tamanoPagina, ct));
    }

    [HttpPost("porcobrar")]
    public async Task<IActionResult> CrearPorCobrar([FromQuery] Guid sucursalId, CrearCuentaPorCobrarRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            var id = await cxcCxp.CrearCuentaPorCobrarAsync(sucursalId, request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("porcobrar/{cuentaId:guid}/pagos")]
    public async Task<IActionResult> PagarPorCobrar([FromQuery] Guid sucursalId, Guid cuentaId, RegistrarPagoRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await cxcCxp.RegistrarPagoCxCAsync(sucursalId, cuentaId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("porpagar")]
    public async Task<IActionResult> ListarPorPagar([FromQuery] Guid sucursalId, [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await cxcCxp.ListarPorPagarAsync(sucursalId, pagina == 0 ? 1 : pagina, tamanoPagina == 0 ? 20 : tamanoPagina, ct));
    }

    [HttpPost("porpagar")]
    public async Task<IActionResult> CrearPorPagar([FromQuery] Guid sucursalId, CrearCuentaPorPagarRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            var id = await cxcCxp.CrearCuentaPorPagarAsync(sucursalId, request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("porpagar/{cuentaId:guid}/pagos")]
    public async Task<IActionResult> PagarPorPagar([FromQuery] Guid sucursalId, Guid cuentaId, RegistrarPagoRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await cxcCxp.RegistrarPagoCxPAsync(sucursalId, cuentaId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
