using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Pedidos;
using SaborByte.Aplicacion.Pedidos.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/mesas")]
[Authorize]
public class MesasController(MesaAppService mesas) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        // Admin ve/gestiona las mesas de CUALQUIER sucursal desde Central (no solo la
        // suya); Mesero/Cocina/Cajero siguen acotados a la(s) sucursal(es) asignada(s).
        if (!User.IsInRole("Admin") && !User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await mesas.ListarAsync(sucursalId, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, GuardarMesaRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await mesas.CrearAsync(sucursalId, request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{mesaId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar([FromQuery] Guid sucursalId, Guid mesaId, GuardarMesaRequestDto request, CancellationToken ct)
    {
        try
        {
            await mesas.ActualizarAsync(sucursalId, mesaId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{mesaId:guid}/liberar")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> Liberar([FromQuery] Guid sucursalId, Guid mesaId, CancellationToken ct)
    {
        if (!User.IsInRole("Admin") && !User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            await mesas.LiberarAsync(sucursalId, mesaId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
