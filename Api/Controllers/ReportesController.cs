using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Reportes;
using SaborByte.Aplicacion.Reportes.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Admin,Supervisor")]
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

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.ObtenerDashboardAsync(sucursalId, ct));
    }

    [HttpPost("ventas-resumen-por-dia")]
    public async Task<IActionResult> VentasResumenPorDia([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.VentasResumenPorDiaAsync(sucursalId, request, ct));
    }

    [HttpPost("ventas-detalle")]
    public async Task<IActionResult> VentasDetalle([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.VentasDetalleAsync(sucursalId, request, ct));
    }

    [HttpPost("ventas-por-categoria")]
    public async Task<IActionResult> VentasPorCategoria([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.VentasPorCategoriaAsync(sucursalId, request, ct));
    }

    [HttpPost("ventas-por-metodo-pago")]
    public async Task<IActionResult> VentasPorMetodoPago([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.VentasPorMetodoPagoAsync(sucursalId, request, ct));
    }

    [HttpPost("movimientos-inventario")]
    public async Task<IActionResult> MovimientosInventario([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.MovimientosInventarioAsync(sucursalId, request, ct));
    }

    [HttpGet("cxc-pendientes")]
    public async Task<IActionResult> CxCPendientes([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.CxCPendientesAsync(sucursalId, ct));
    }

    [HttpGet("cxp-pendientes")]
    public async Task<IActionResult> CxPPendientes([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.CxPPendientesAsync(sucursalId, ct));
    }

    [HttpPost("cxc-pagos")]
    public async Task<IActionResult> CxCPagos([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.CxCPagosAsync(sucursalId, request, ct));
    }

    [HttpPost("cxp-pagos")]
    public async Task<IActionResult> CxPPagos([FromQuery] Guid sucursalId, RangoFechasRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await reportes.CxPPagosAsync(sucursalId, request, ct));
    }
}
