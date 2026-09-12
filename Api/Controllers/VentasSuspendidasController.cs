using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Ventas;
using SaborByte.Aplicacion.Ventas.Dtos;

namespace SaborByte.Api.Controllers;

// Cualquier Cajero puede guardar/recuperar/eliminar — es una operación de rutina de Caja,
// igual criterio que asignar una factura a un delivery.
[ApiController]
[Route("api/ventas-suspendidas")]
[Authorize]
public class VentasSuspendidasController(VentaSuspendidaAppService ventasSuspendidas) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await ventasSuspendidas.ListarAsync(sucursalId, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener([FromQuery] Guid sucursalId, Guid id, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            return Ok(await ventasSuspendidas.ObtenerAsync(sucursalId, id, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromQuery] Guid sucursalId, GuardarVentaSuspendidaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            var id = await ventasSuspendidas.GuardarAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
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
            await ventasSuspendidas.EliminarAsync(sucursalId, id, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
