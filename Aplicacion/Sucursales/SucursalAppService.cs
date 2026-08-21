using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Sucursales.Dtos;

namespace SaborByte.Aplicacion.Sucursales;

public class SucursalAppService(IAppDbContext db)
{
    public async Task<SucursalDto> ObtenerAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == sucursalId, ct)
            ?? throw new InvalidOperationException("La sucursal no existe.");

        return new SucursalDto
        {
            Id = sucursal.Id,
            Nombre = sucursal.Nombre,
            Rnc = sucursal.Rnc,
            Direccion = sucursal.Direccion,
            Telefono = sucursal.Telefono,
            ModuloMeseroActivo = sucursal.ModuloMeseroActivo,
            ModuloCocinaActivo = sucursal.ModuloCocinaActivo,
            EcfActivo = sucursal.EcfActivo,
            SmtpActivo = sucursal.SmtpActivo,
            SmtpHost = sucursal.SmtpHost,
            SmtpPuerto = sucursal.SmtpPuerto,
            SmtpUsuario = sucursal.SmtpUsuario,
            SmtpRemitente = sucursal.SmtpRemitente,
            SmtpUsaSsl = sucursal.SmtpUsaSsl
        };
    }

    public async Task ActualizarAsync(Guid sucursalId, ActualizarSucursalRequestDto request, CancellationToken ct = default)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == sucursalId, ct)
            ?? throw new InvalidOperationException("La sucursal no existe.");

        sucursal.Nombre = request.Nombre;
        sucursal.Rnc = request.Rnc;
        sucursal.Direccion = request.Direccion;
        sucursal.Telefono = request.Telefono;
        sucursal.ModuloMeseroActivo = request.ModuloMeseroActivo;
        sucursal.ModuloCocinaActivo = request.ModuloCocinaActivo;
        sucursal.EcfActivo = request.EcfActivo;

        sucursal.SmtpActivo = request.SmtpActivo;
        sucursal.SmtpHost = request.SmtpHost;
        sucursal.SmtpPuerto = request.SmtpPuerto;
        sucursal.SmtpUsuario = request.SmtpUsuario;
        sucursal.SmtpRemitente = request.SmtpRemitente;
        sucursal.SmtpUsaSsl = request.SmtpUsaSsl;
        if (!string.IsNullOrEmpty(request.SmtpPassword))
            sucursal.SmtpPassword = request.SmtpPassword;

        await db.SaveChangesAsync(ct);
    }
}
