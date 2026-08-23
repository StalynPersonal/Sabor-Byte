using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.Identidad.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Identidad;

namespace SaborByte.Aplicacion.Identidad;

public class UsuarioAppService(IAppDbContext db, IPasswordHasher passwordHasher)
{
    public async Task<ResultadoPaginado<UsuarioDto>> ListarAsync(int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.Usuarios
            .Include(u => u.Roles).ThenInclude(r => r.Rol)
            .Include(u => u.SucursalesAsignadas)
            .OrderBy(u => u.NombreUsuario);

        var total = await query.CountAsync(ct);

        var usuarios = await query
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        var idAdminPrincipal = await ObtenerIdAdminPrincipalAsync(ct);

        return new ResultadoPaginado<UsuarioDto>
        {
            Items = usuarios.Select(u => MapearDto(u, idAdminPrincipal)).ToList(),
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = total
        };
    }

    // El primer usuario creado en todo el sistema (el admin de la siembra inicial) es
    // intocable: sin él, un Admin podría editarse/desactivarse a sí mismo (o a otro
    // Admin) por error y dejar el sistema sin nadie que pueda administrar usuarios.
    // Se identifica por antigüedad (CreadoEn), no por nombre de usuario, para que siga
    // funcionando aunque lo rebauticen.
    private async Task<Guid> ObtenerIdAdminPrincipalAsync(CancellationToken ct) =>
        await db.Usuarios.OrderBy(u => u.CreadoEn).Select(u => u.Id).FirstAsync(ct);

    public async Task<Guid> CrearAsync(Guid usuarioActorId, CrearUsuarioRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario))
            throw new InvalidOperationException("El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre completo es obligatorio.");

        var existe = await db.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario, ct);
        if (existe)
            throw new InvalidOperationException("Ya existe un usuario con ese nombre de usuario.");

        // Un usuario sin sucursal asignada no podría acceder a ningún dato al iniciar
        // sesión (SucursalesPermitidas quedaría vacío) — se rechaza aquí, no después.
        if (request.SucursalesAsignadas.Count == 0)
            throw new InvalidOperationException("El usuario debe tener al menos una sucursal asignada.");

        var roles = await db.Roles.Where(r => request.Roles.Contains(r.Nombre)).ToListAsync(ct);

        var usuario = new Usuario
        {
            NombreUsuario = request.NombreUsuario,
            Nombre = request.Nombre,
            Email = request.Email,
            HashPassword = passwordHasher.Hash(request.Password),
            CreadoPorUsuarioId = usuarioActorId
        };

        foreach (var rol in roles)
            usuario.Roles.Add(new UsuarioRol { Usuario = usuario, Rol = rol });

        foreach (var sucursalId in request.SucursalesAsignadas)
            usuario.SucursalesAsignadas.Add(new UsuarioSucursal { Usuario = usuario, SucursalId = sucursalId });

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        return usuario.Id;
    }

    public async Task ActualizarAsync(Guid usuarioId, Guid usuarioActorId, ActualizarUsuarioRequestDto request, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Roles)
            .Include(u => u.SucursalesAsignadas)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, ct)
            ?? throw new InvalidOperationException("El usuario no existe.");

        if (usuarioId == await ObtenerIdAdminPrincipalAsync(ct))
            throw new InvalidOperationException("El administrador principal no se puede editar.");

        if (string.IsNullOrWhiteSpace(request.NombreUsuario))
            throw new InvalidOperationException("El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre completo es obligatorio.");

        var enUsoPorOtro = await db.Usuarios.AnyAsync(u => u.Id != usuarioId && u.NombreUsuario == request.NombreUsuario, ct);
        if (enUsoPorOtro)
            throw new InvalidOperationException("Ya existe un usuario con ese nombre de usuario.");

        if (request.SucursalesAsignadas.Count == 0)
            throw new InvalidOperationException("El usuario debe tener al menos una sucursal asignada.");

        usuario.NombreUsuario = request.NombreUsuario;
        usuario.Nombre = request.Nombre;
        usuario.Email = request.Email;
        usuario.Activo = request.Activo;

        if (!string.IsNullOrWhiteSpace(request.Password))
            usuario.HashPassword = passwordHasher.Hash(request.Password);

        var roles = await db.Roles.Where(r => request.Roles.Contains(r.Nombre)).ToListAsync(ct);
        usuario.Roles.Clear();
        foreach (var rol in roles)
            usuario.Roles.Add(new UsuarioRol { Usuario = usuario, Rol = rol });

        usuario.SucursalesAsignadas.Clear();
        foreach (var sucursalId in request.SucursalesAsignadas)
            usuario.SucursalesAsignadas.Add(new UsuarioSucursal { Usuario = usuario, SucursalId = sucursalId });

        usuario.ActualizadoEn = DateTime.UtcNow;
        usuario.ActualizadoPorUsuarioId = usuarioActorId;

        await db.SaveChangesAsync(ct);
    }

    // Autoservicio: cualquier usuario autenticado cambia su propia contraseña (no requiere
    // rol Admin), pero debe reconfirmar la actual — sin eso, alguien que deja la sesión
    // abierta en una tablet compartida podría cambiarle la contraseña a otro.
    public async Task CambiarPasswordAsync(Guid usuarioId, CambiarPasswordRequestDto request, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct)
            ?? throw new InvalidOperationException("El usuario no existe.");

        if (!passwordHasher.Verificar(usuario.HashPassword, request.PasswordActual))
            throw new InvalidOperationException("La contraseña actual no es correcta.");

        if (string.IsNullOrWhiteSpace(request.PasswordNueva) || request.PasswordNueva.Length < 8)
            throw new InvalidOperationException("La nueva contraseña debe tener al menos 8 caracteres.");

        usuario.HashPassword = passwordHasher.Hash(request.PasswordNueva);
        usuario.ActualizadoEn = DateTime.UtcNow;
        usuario.ActualizadoPorUsuarioId = usuarioId;
        await db.SaveChangesAsync(ct);
    }

    public async Task DesactivarAsync(Guid usuarioId, Guid usuarioActorId, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct)
            ?? throw new InvalidOperationException("El usuario no existe.");

        if (usuarioId == await ObtenerIdAdminPrincipalAsync(ct))
            throw new InvalidOperationException("El administrador principal no se puede desactivar.");

        usuario.Activo = false;
        usuario.ActualizadoEn = DateTime.UtcNow;
        usuario.ActualizadoPorUsuarioId = usuarioActorId;
        await db.SaveChangesAsync(ct);
    }

    private static UsuarioDto MapearDto(Usuario u, Guid idAdminPrincipal) => new()
    {
        Id = u.Id,
        NombreUsuario = u.NombreUsuario,
        Nombre = u.Nombre,
        Email = u.Email,
        Activo = u.Activo,
        Roles = u.Roles.Select(r => r.Rol!.Nombre).ToList(),
        SucursalesAsignadas = u.SucursalesAsignadas.Select(s => s.SucursalId).ToList(),
        EsAdminPrincipal = u.Id == idAdminPrincipal
    };
}
