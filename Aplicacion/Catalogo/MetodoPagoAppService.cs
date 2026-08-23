using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Catalogo.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo;

// Catálogo global (no por sucursal) — Admin lo administra desde Central; el resto de
// la app solo lo lee para poblar selectores de forma de pago (Caja, CxC/CxP).
public class MetodoPagoAppService(IAppDbContext db)
{
    public async Task<List<MetodoPagoDto>> ListarAsync(bool incluirInactivos, CancellationToken ct = default) =>
        await db.MetodosPago
            .Where(m => incluirInactivos || m.Activo)
            .OrderBy(m => m.Nombre)
            .Select(m => new MetodoPagoDto
            {
                Id = m.Id,
                Nombre = m.Nombre,
                EsEfectivo = m.EsEfectivo,
                RequiereComprobante = m.RequiereComprobante,
                Activo = m.Activo
            })
            .ToListAsync(ct);

    public async Task<Guid> CrearAsync(GuardarMetodoPagoRequestDto request, CancellationToken ct = default)
    {
        var yaExiste = await db.MetodosPago.AnyAsync(m => m.Nombre == request.Nombre, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe una forma de pago llamada '{request.Nombre}'.");

        var metodo = new MetodoPago
        {
            Nombre = request.Nombre,
            EsEfectivo = request.EsEfectivo,
            RequiereComprobante = request.RequiereComprobante,
            Activo = request.Activo
        };
        db.MetodosPago.Add(metodo);
        await db.SaveChangesAsync(ct);
        return metodo.Id;
    }

    public async Task ActualizarAsync(Guid metodoPagoId, GuardarMetodoPagoRequestDto request, CancellationToken ct = default)
    {
        var metodo = await db.MetodosPago.FirstOrDefaultAsync(m => m.Id == metodoPagoId, ct)
            ?? throw new InvalidOperationException("La forma de pago no existe.");

        var yaExiste = await db.MetodosPago.AnyAsync(m => m.Id != metodoPagoId && m.Nombre == request.Nombre, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe otra forma de pago llamada '{request.Nombre}'.");

        metodo.Nombre = request.Nombre;
        metodo.EsEfectivo = request.EsEfectivo;
        metodo.RequiereComprobante = request.RequiereComprobante;
        metodo.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
    }
}
