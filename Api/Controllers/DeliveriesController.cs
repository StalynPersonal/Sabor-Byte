using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Deliveries;
using SaborByte.Aplicacion.Deliveries.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/deliveries")]
[Authorize]
public class DeliveriesController(DeliveryAppService deliveries) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid sucursalId, [FromQuery] bool incluirInactivos, [FromQuery] string? texto, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await deliveries.ListarAsync(sucursalId, incluirInactivos, texto, ct));
    }

    [HttpGet("todos")]
    public async Task<IActionResult> ListarPaginado(
        [FromQuery] Guid sucursalId, [FromQuery] bool incluirInactivos, [FromQuery] string? texto,
        [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await deliveries.ListarPaginadoAsync(sucursalId, incluirInactivos, texto, pagina, tamanoPagina, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, GuardarDeliveryRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            var id = await deliveries.CrearAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{deliveryId:guid}")]
    public async Task<IActionResult> Actualizar([FromQuery] Guid sucursalId, Guid deliveryId, GuardarDeliveryRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await deliveries.ActualizarAsync(sucursalId, deliveryId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{deliveryId:guid}")]
    public async Task<IActionResult> Desactivar([FromQuery] Guid sucursalId, Guid deliveryId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await deliveries.DesactivarAsync(sucursalId, deliveryId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("facturas/buscar")]
    public async Task<IActionResult> BuscarFacturasAsignables([FromQuery] Guid sucursalId, [FromQuery] string? texto, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await deliveries.BuscarFacturasAsignablesAsync(sucursalId, texto, ct));
    }

    [HttpGet("{deliveryId:guid}/resumen-cuenta")]
    public async Task<IActionResult> ObtenerResumenCuenta([FromQuery] Guid sucursalId, Guid deliveryId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            return Ok(await deliveries.ObtenerResumenCuentaAsync(sucursalId, deliveryId, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{deliveryId:guid}/facturas")]
    public async Task<IActionResult> ListarFacturasAsignadas(
        [FromQuery] Guid sucursalId, Guid deliveryId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            return Ok(await deliveries.ListarFacturasAsignadasAsync(sucursalId, deliveryId, desde, hasta, pagina, tamanoPagina, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    // Accesible a cualquier Cajero: asignar una factura ya emitida a un delivery es una
    // operación de rutina, siempre posterior a la venta (nunca al momento de facturar).
    [HttpPost("{deliveryId:guid}/facturas")]
    public async Task<IActionResult> AsignarFactura([FromQuery] Guid sucursalId, Guid deliveryId, AsignarFacturaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await deliveries.AsignarFacturaAsync(sucursalId, deliveryId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // Quitar una asignación es una corrección de error, no una operación de rutina.
    [HttpDelete("{deliveryId:guid}/facturas/{facturaDeliveryId:guid}")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> QuitarAsignacion([FromQuery] Guid sucursalId, Guid deliveryId, Guid facturaDeliveryId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await deliveries.QuitarAsignacionAsync(sucursalId, deliveryId, facturaDeliveryId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{deliveryId:guid}/abonos")]
    public async Task<IActionResult> ListarAbonos(
        [FromQuery] Guid sucursalId, Guid deliveryId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] int pagina, [FromQuery] int tamanoPagina, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            return Ok(await deliveries.ListarAbonosAsync(sucursalId, deliveryId, desde, hasta, pagina, tamanoPagina, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    // Registrar el abono lo puede hacer cualquier Cajero (recibe el efectivo del delivery
    // en el día a día) — anular ya registrado o quitar una asignación siguen restringidas
    // a Admin/Supervisor, por ser correcciones.
    [HttpPost("{deliveryId:guid}/abonos")]
    [Authorize(Roles = "Admin,Supervisor,Cajero")]
    public async Task<IActionResult> RegistrarAbono([FromQuery] Guid sucursalId, Guid deliveryId, RegistrarAbonoDeliveryRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await deliveries.RegistrarAbonoAsync(sucursalId, deliveryId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{deliveryId:guid}/abonos/{abonoId:guid}/anular")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> AnularAbono([FromQuery] Guid sucursalId, Guid deliveryId, Guid abonoId, AnularAbonoDeliveryRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await deliveries.AnularAbonoAsync(sucursalId, deliveryId, abonoId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
