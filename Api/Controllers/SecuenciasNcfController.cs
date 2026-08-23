using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Facturacion;
using SaborByte.Aplicacion.Facturacion.Dtos;

namespace SaborByte.Api.Controllers;

// Configuración de las secuencias NCF/e-CF (usadas tanto por Facturas como por Notas de
// Crédito/Débito) — solo Admin, es dato fiscal sensible.
[ApiController]
[Route("api/secuenciasncf")]
[Authorize(Roles = "Admin")]
public class SecuenciasNcfController(SecuenciaNcfAppService secuencias) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid sucursalId, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        return Ok(await secuencias.ListarAsync(sucursalId, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromQuery] Guid sucursalId, GuardarSecuenciaNcfRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            var id = await secuencias.CrearAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{secuenciaId:guid}")]
    public async Task<IActionResult> Actualizar([FromQuery] Guid sucursalId, Guid secuenciaId, GuardarSecuenciaNcfRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId))
            return Forbid();

        try
        {
            await secuencias.ActualizarAsync(sucursalId, secuenciaId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
