using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaborByte.Aplicacion.Sucursales;
using SaborByte.Aplicacion.Sucursales.Dtos;

namespace SaborByte.Api.Controllers;

// Singleton: solo existe una Empresa en todo el sistema (multisucursal, no multiempresa).
// Cualquier usuario autenticado puede leerla (se muestra en el AppBar); solo Admin la edita.
[ApiController]
[Route("api/empresa")]
[Authorize]
public class EmpresasController(EmpresaAppService empresa) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken ct) => Ok(await empresa.ObtenerAsync(ct));

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(GuardarEmpresaRequestDto request, CancellationToken ct)
    {
        try
        {
            await empresa.ActualizarAsync(request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
