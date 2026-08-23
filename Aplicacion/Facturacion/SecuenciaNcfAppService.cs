using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Facturacion.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Facturacion;

namespace SaborByte.Aplicacion.Facturacion;

public class SecuenciaNcfAppService(IAppDbContext db)
{
    // Tipos de comprobante fiscal reconocidos por DGII (con o sin e-CF). Se valida contra
    // esta lista para no dejar registrar un tipo inventado que después nadie use.
    private static readonly HashSet<string> TiposValidos = ["31", "32", "33", "34", "41", "43", "44", "45"];

    public async Task<List<SecuenciaNcfDto>> ListarAsync(Guid sucursalId, CancellationToken ct = default) =>
        await db.SecuenciasNcf
            .Where(s => s.SucursalId == sucursalId)
            .OrderBy(s => s.TipoComprobante)
            .Select(s => MapearDto(s))
            .ToListAsync(ct);

    public async Task<Guid> CrearAsync(Guid sucursalId, Guid usuarioId, GuardarSecuenciaNcfRequestDto request, CancellationToken ct = default)
    {
        ValidarRequest(request);

        var yaExiste = await db.SecuenciasNcf.AnyAsync(s =>
            s.SucursalId == sucursalId && s.TipoComprobante == request.TipoComprobante && s.Activa, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe una secuencia activa para el tipo de comprobante '{request.TipoComprobante}' en esta sucursal.");

        var secuencia = new SecuenciaNcf
        {
            SucursalId = sucursalId,
            Serie = request.Serie,
            TipoComprobante = request.TipoComprobante,
            SecuenciaInicial = request.SecuenciaInicial,
            SecuenciaProxima = request.SecuenciaProxima < request.SecuenciaInicial ? request.SecuenciaInicial : request.SecuenciaProxima,
            SecuenciaFinal = request.SecuenciaFinal,
            FechaVencimiento = request.FechaVencimiento,
            Activa = request.Activa,
            CreadoPorUsuarioId = usuarioId
        };

        db.SecuenciasNcf.Add(secuencia);
        await db.SaveChangesAsync(ct);
        return secuencia.Id;
    }

    public async Task ActualizarAsync(Guid sucursalId, Guid secuenciaId, Guid usuarioId, GuardarSecuenciaNcfRequestDto request, CancellationToken ct = default)
    {
        ValidarRequest(request);

        var secuencia = await db.SecuenciasNcf.FirstOrDefaultAsync(s => s.Id == secuenciaId && s.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La secuencia no existe.");

        // Nunca se permite retroceder: bajarla colisionaría con NCF que ya se emitieron
        // (el índice único de Factura.NumeroNcf lo rechazaría en producción; mejor avisar aquí).
        if (request.SecuenciaProxima < secuencia.SecuenciaProxima)
            throw new InvalidOperationException(
                $"La secuencia próxima no puede retroceder (actual: {secuencia.SecuenciaProxima}). Solo puede mantenerse o avanzar.");

        if (request.TipoComprobante != secuencia.TipoComprobante)
        {
            var yaExiste = await db.SecuenciasNcf.AnyAsync(s =>
                s.Id != secuenciaId && s.SucursalId == sucursalId && s.TipoComprobante == request.TipoComprobante && s.Activa, ct);
            if (yaExiste)
                throw new InvalidOperationException($"Ya existe otra secuencia activa para el tipo de comprobante '{request.TipoComprobante}' en esta sucursal.");
        }

        secuencia.Serie = request.Serie;
        secuencia.TipoComprobante = request.TipoComprobante;
        secuencia.SecuenciaInicial = request.SecuenciaInicial;
        secuencia.SecuenciaProxima = request.SecuenciaProxima;
        secuencia.SecuenciaFinal = request.SecuenciaFinal;
        secuencia.FechaVencimiento = request.FechaVencimiento;
        secuencia.Activa = request.Activa;
        secuencia.ActualizadoEn = DateTime.UtcNow;
        secuencia.ActualizadoPorUsuarioId = usuarioId;

        await db.SaveChangesAsync(ct);
    }

    private static void ValidarRequest(GuardarSecuenciaNcfRequestDto request)
    {
        if (!TiposValidos.Contains(request.TipoComprobante))
            throw new InvalidOperationException(
                $"Tipo de comprobante '{request.TipoComprobante}' no reconocido. Válidos: {string.Join(", ", TiposValidos)}.");

        if (string.IsNullOrWhiteSpace(request.Serie) || request.Serie.Length > 5)
            throw new InvalidOperationException("La serie debe tener entre 1 y 5 caracteres (ej. \"E\").");

        if (request.SecuenciaFinal <= request.SecuenciaInicial)
            throw new InvalidOperationException("La secuencia final debe ser mayor que la inicial.");
    }

    private static SecuenciaNcfDto MapearDto(SecuenciaNcf s) => new()
    {
        Id = s.Id,
        Serie = s.Serie,
        TipoComprobante = s.TipoComprobante,
        SecuenciaInicial = s.SecuenciaInicial,
        SecuenciaProxima = s.SecuenciaProxima,
        SecuenciaFinal = s.SecuenciaFinal,
        FechaVencimiento = s.FechaVencimiento,
        Activa = s.Activa
    };
}
