using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Facturacion;
using SaborByte.Aplicacion.Facturacion.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/notascreditodebito")]
[Authorize]
public class NotasCreditoDebitoController(NotaCreditoDebitoAppService notas) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Supervisor,Admin")]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, CrearNotaRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            return Ok(await notas.CrearAsync(sucursalId, User.ObtenerUsuarioId(), request, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("por-factura/{facturaId:guid}")]
    public async Task<IActionResult> ListarPorFactura([FromQuery] Guid sucursalId, Guid facturaId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await notas.ListarPorFacturaAsync(sucursalId, facturaId, ct));
    }
}
