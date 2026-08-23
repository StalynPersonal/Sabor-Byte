using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Sucursales.Dtos;
using SaborByte.Dominio.Identidad;
using SaborByte.Dominio.Sucursales;

namespace SaborByte.Aplicacion.Sucursales;

public class SucursalAppService(IAppDbContext db, IAuditoriaService auditoria)
{
    // Admin: para la pantalla de gestión (crear sucursales nuevas, ver todas).
    public async Task<List<SucursalResumenDto>> ListarTodasAsync(CancellationToken ct = default) =>
        await db.Sucursales
            .OrderBy(s => s.Nombre)
            .Select(s => new SucursalResumenDto
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Codigo = s.Codigo,
                Activa = s.Activa
            })
            .ToListAsync(ct);

    // Crea la sucursal y asigna de una vez al usuario que la creó (si no, quedaría
    // creada pero invisible para él: el acceso se otorga vía UsuarioSucursal, no por
    // ser Admin — sin esto, un admin creaba una sucursal y no podía ni verla después).
    public async Task<Guid> CrearAsync(Guid usuarioCreadorId, CrearSucursalRequestDto request, CancellationToken ct = default)
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No hay una empresa configurada en el sistema.");

        if (string.IsNullOrWhiteSpace(request.Codigo) || request.Codigo.Length != 2 || !request.Codigo.All(char.IsDigit))
            throw new InvalidOperationException("El código de sucursal debe ser numérico de exactamente 2 dígitos (ej. \"01\").");

        var codigoEnUso = await db.Sucursales.AnyAsync(s => s.Codigo == request.Codigo, ct);
        if (codigoEnUso)
            throw new InvalidOperationException($"Ya existe una sucursal con el código '{request.Codigo}'.");

        var sucursal = new Sucursal
        {
            EmpresaId = empresa.Id,
            Nombre = request.Nombre,
            Codigo = request.Codigo,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            ModuloMeseroActivo = request.ModuloMeseroActivo,
            ModuloCocinaActivo = request.ModuloCocinaActivo,
            EcfActivo = request.EcfActivo,
            CreadoPorUsuarioId = usuarioCreadorId
        };

        db.Sucursales.Add(sucursal);

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioCreadorId, ct);
        usuario?.SucursalesAsignadas.Add(new UsuarioSucursal { UsuarioId = usuarioCreadorId, SucursalId = sucursal.Id });

        // Sucursal nueva: arranca en 0 el stock de cada insumo ya existente en el catálogo
        // (que es de toda la empresa) — cada sucursal configura su propio mínimo/máximo
        // y entrada inicial después, desde Inventario.
        var insumoIds = await db.Productos
            .Where(p => p.TipoProducto == Dominio.Catalogo.TipoProducto.Insumo)
            .Select(p => p.Id)
            .ToListAsync(ct);
        db.StockPorSucursal.AddRange(insumoIds.Select(pid => new Dominio.Catalogo.StockSucursal
        {
            ProductoId = pid,
            SucursalId = sucursal.Id
        }));

        await db.SaveChangesAsync(ct);
        return sucursal.Id;
    }

    public async Task<SucursalDto> ObtenerAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == sucursalId, ct)
            ?? throw new InvalidOperationException("La sucursal no existe.");

        return new SucursalDto
        {
            Id = sucursal.Id,
            Nombre = sucursal.Nombre,
            Codigo = sucursal.Codigo,
            Direccion = sucursal.Direccion,
            Telefono = sucursal.Telefono,
            Activa = sucursal.Activa,
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

    public async Task ActualizarAsync(Guid sucursalId, Guid usuarioId, ActualizarSucursalRequestDto request, CancellationToken ct = default)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == sucursalId, ct)
            ?? throw new InvalidOperationException("La sucursal no existe.");

        if (string.IsNullOrWhiteSpace(request.Codigo) || request.Codigo.Length != 2 || !request.Codigo.All(char.IsDigit))
            throw new InvalidOperationException("El código de sucursal debe ser numérico de exactamente 2 dígitos (ej. \"01\").");

        var codigoEnUso = await db.Sucursales.AnyAsync(s => s.Id != sucursalId && s.Codigo == request.Codigo, ct);
        if (codigoEnUso)
            throw new InvalidOperationException($"Ya existe otra sucursal con el código '{request.Codigo}'.");

        sucursal.Nombre = request.Nombre;
        sucursal.Codigo = request.Codigo;
        sucursal.Direccion = request.Direccion;
        sucursal.Telefono = request.Telefono;
        sucursal.Activa = request.Activa;
        sucursal.ModuloMeseroActivo = request.ModuloMeseroActivo;
        sucursal.ModuloCocinaActivo = request.ModuloCocinaActivo;
        sucursal.EcfActivo = request.EcfActivo;
        sucursal.ActualizadoEn = DateTime.UtcNow;
        sucursal.ActualizadoPorUsuarioId = usuarioId;

        await db.SaveChangesAsync(ct);
        await auditoria.RegistrarAsync(sucursalId, usuarioId, "CambioConfiguracion", "Sucursal", sucursalId, ct: ct);
    }

    // Correo saliente (SMTP): pantalla propia dentro de "Administración", separada del
    // formulario general de la sucursal — por eso vive en su propio método/endpoint,
    // que solo toca estos campos y no pisa el resto de la configuración al guardar.
    public async Task ActualizarSmtpAsync(Guid sucursalId, Guid usuarioId, ActualizarSmtpRequestDto request, CancellationToken ct = default)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == sucursalId, ct)
            ?? throw new InvalidOperationException("La sucursal no existe.");

        sucursal.SmtpActivo = request.SmtpActivo;
        sucursal.SmtpHost = request.SmtpHost;
        sucursal.SmtpPuerto = request.SmtpPuerto;
        sucursal.SmtpUsuario = request.SmtpUsuario;
        sucursal.SmtpRemitente = request.SmtpRemitente;
        sucursal.SmtpUsaSsl = request.SmtpUsaSsl;
        if (!string.IsNullOrEmpty(request.SmtpPassword))
            sucursal.SmtpPassword = request.SmtpPassword;

        sucursal.ActualizadoEn = DateTime.UtcNow;
        sucursal.ActualizadoPorUsuarioId = usuarioId;

        await db.SaveChangesAsync(ct);
        await auditoria.RegistrarAsync(sucursalId, usuarioId, "CambioConfiguracion", "Sucursal.Smtp", sucursalId, ct: ct);
    }
}
