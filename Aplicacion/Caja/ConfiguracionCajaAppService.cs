using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Caja.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Caja;

namespace SaborByte.Aplicacion.Caja;

// Catálogos globales (no por sucursal) — Admin los administra desde Central; Caja solo
// los lee para poblar el selector de propina y el desglose de efectivo del cierre de
// turno (antes eran listas fijas en código).
public class ConfiguracionCajaAppService(IAppDbContext db)
{
    public async Task<List<PorcentajePropinaDto>> ListarPorcentajesPropinaAsync(bool incluirInactivos, CancellationToken ct = default) =>
        await db.PorcentajesPropina
            .Where(p => incluirInactivos || p.Activo)
            .OrderBy(p => p.Valor)
            .Select(p => new PorcentajePropinaDto { Id = p.Id, Valor = p.Valor, Activo = p.Activo })
            .ToListAsync(ct);

    public async Task<Guid> CrearPorcentajePropinaAsync(GuardarPorcentajePropinaRequestDto request, CancellationToken ct = default)
    {
        if (request.Valor < 0 || request.Valor > 100)
            throw new InvalidOperationException("El porcentaje de propina debe estar entre 0 y 100.");

        var yaExiste = await db.PorcentajesPropina.AnyAsync(p => p.Valor == request.Valor, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe un porcentaje de propina de {request.Valor}%.");

        var porcentaje = new PorcentajePropina { Valor = request.Valor, Activo = request.Activo };
        db.PorcentajesPropina.Add(porcentaje);
        await db.SaveChangesAsync(ct);
        return porcentaje.Id;
    }

    public async Task ActualizarPorcentajePropinaAsync(Guid porcentajeId, GuardarPorcentajePropinaRequestDto request, CancellationToken ct = default)
    {
        if (request.Valor < 0 || request.Valor > 100)
            throw new InvalidOperationException("El porcentaje de propina debe estar entre 0 y 100.");

        var porcentaje = await db.PorcentajesPropina.FirstOrDefaultAsync(p => p.Id == porcentajeId, ct)
            ?? throw new InvalidOperationException("El porcentaje de propina no existe.");

        var yaExiste = await db.PorcentajesPropina.AnyAsync(p => p.Id != porcentajeId && p.Valor == request.Valor, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe otro porcentaje de propina de {request.Valor}%.");

        porcentaje.Valor = request.Valor;
        porcentaje.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<DenominacionEfectivoDto>> ListarDenominacionesEfectivoAsync(bool incluirInactivos, CancellationToken ct = default) =>
        await db.DenominacionesEfectivo
            .Where(d => incluirInactivos || d.Activo)
            .OrderByDescending(d => d.Valor)
            .Select(d => new DenominacionEfectivoDto { Id = d.Id, Valor = d.Valor, Activo = d.Activo })
            .ToListAsync(ct);

    public async Task<Guid> CrearDenominacionEfectivoAsync(GuardarDenominacionEfectivoRequestDto request, CancellationToken ct = default)
    {
        if (request.Valor <= 0)
            throw new InvalidOperationException("El valor de la denominación debe ser mayor a cero.");

        var yaExiste = await db.DenominacionesEfectivo.AnyAsync(d => d.Valor == request.Valor, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe una denominación de RD$ {request.Valor}.");

        var denominacion = new DenominacionEfectivo { Valor = request.Valor, Activo = request.Activo };
        db.DenominacionesEfectivo.Add(denominacion);
        await db.SaveChangesAsync(ct);
        return denominacion.Id;
    }

    public async Task ActualizarDenominacionEfectivoAsync(Guid denominacionId, GuardarDenominacionEfectivoRequestDto request, CancellationToken ct = default)
    {
        if (request.Valor <= 0)
            throw new InvalidOperationException("El valor de la denominación debe ser mayor a cero.");

        var denominacion = await db.DenominacionesEfectivo.FirstOrDefaultAsync(d => d.Id == denominacionId, ct)
            ?? throw new InvalidOperationException("La denominación no existe.");

        var yaExiste = await db.DenominacionesEfectivo.AnyAsync(d => d.Id != denominacionId && d.Valor == request.Valor, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe otra denominación de RD$ {request.Valor}.");

        denominacion.Valor = request.Valor;
        denominacion.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
    }
}
