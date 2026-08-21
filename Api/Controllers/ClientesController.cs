using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Clientes;
using SaborByte.Aplicacion.Clientes.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController(ClienteAppService clientes) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] Guid sucursalId, [FromQuery] string? texto, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await clientes.BuscarAsync(sucursalId, texto, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, GuardarClienteRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        var id = await clientes.CrearAsync(sucursalId, request, ct);
        return Ok(new { id });
    }

    [HttpPut("{clienteId:guid}")]
    public async Task<IActionResult> Actualizar(Guid clienteId, GuardarClienteRequestDto request, CancellationToken ct)
    {
        try
        {
            await clientes.ActualizarAsync(clienteId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{clienteId:guid}")]
    public async Task<IActionResult> Desactivar(Guid clienteId, CancellationToken ct)
    {
        try
        {
            await clientes.DesactivarAsync(clienteId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
