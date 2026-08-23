using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Sucursales.Dtos;

namespace SaborByte.Aplicacion.Sucursales;

// Sistema multisucursal, NO multiempresa: solo existe una fila de Empresa en todo el
// sistema (sembrada una única vez). No hay Crear ni Listar, solo Obtener/Actualizar la
// única fila — es la contraparte "global" de las varias Sucursales que sí se administran
// con un CRUD completo (ver SucursalAppService).
public class EmpresaAppService(IAppDbContext db)
{
    public async Task<EmpresaDto> ObtenerAsync(CancellationToken ct = default)
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No hay una empresa configurada en el sistema.");

        return new EmpresaDto { Id = empresa.Id, Nombre = empresa.Nombre, Rnc = empresa.Rnc };
    }

    public async Task ActualizarAsync(GuardarEmpresaRequestDto request, CancellationToken ct = default)
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No hay una empresa configurada en el sistema.");

        empresa.Nombre = request.Nombre;
        empresa.Rnc = request.Rnc;
        await db.SaveChangesAsync(ct);
    }
}
