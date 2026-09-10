using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Api.Extensiones;
using SaborByte.Aplicacion.Gastos;
using SaborByte.Aplicacion.Gastos.Dtos;

namespace SaborByte.Api.Controllers;

[ApiController]
[Route("api/gastos")]
[Authorize]
public class GastosController(GastoAppService gastos) : ControllerBase
{
    [HttpGet("categorias")]
    public async Task<IActionResult> ListarCategorias([FromQuery] bool incluirInactivas, CancellationToken ct) =>
        Ok(await gastos.ListarCategoriasAsync(incluirInactivas, ct));

    [HttpPost("categorias")]
    public async Task<IActionResult> CrearCategoria(GuardarCategoriaGastoRequestDto request, CancellationToken ct)
    {
        try
        {
            var id = await gastos.CrearCategoriaAsync(request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("categorias/{categoriaGastoId:guid}")]
    public async Task<IActionResult> ActualizarCategoria(Guid categoriaGastoId, GuardarCategoriaGastoRequestDto request, CancellationToken ct)
    {
        try
        {
            await gastos.ActualizarCategoriaAsync(categoriaGastoId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(
        [FromQuery] Guid sucursalId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta,
        [FromQuery] Guid? categoriaGastoId, [FromQuery] string? texto, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await gastos.BuscarAsync(sucursalId, desde, hasta, categoriaGastoId, texto, ct));
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen(
        [FromQuery] Guid sucursalId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();
        return Ok(await gastos.ObtenerResumenAsync(sucursalId, desde, hasta, ct));
    }

    // Registrar un gasto mueve dinero real (incluso cuando descuenta de una caja) — mismo
    // criterio que abonos de delivery y pagos de CxC/CxP: solo Admin/Supervisor, tanto si
    // se hace desde Central como desde Caja.
    [HttpPost]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> Registrar([FromQuery] Guid sucursalId, RegistrarGastoRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            var id = await gastos.RegistrarAsync(sucursalId, User.ObtenerUsuarioId(), request, ct);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // Anular es una corrección de dinero ya registrado — mismo criterio que anular pagos
    // de CxC/CxP y abonos de delivery: solo Admin/Supervisor.
    [HttpPost("{gastoId:guid}/anular")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> Anular([FromQuery] Guid sucursalId, Guid gastoId, AnularGastoRequestDto request, CancellationToken ct)
    {
        if (!User.TieneAccesoASucursal(sucursalId)) return Forbid();

        try
        {
            await gastos.AnularAsync(sucursalId, gastoId, User.ObtenerUsuarioId(), request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
