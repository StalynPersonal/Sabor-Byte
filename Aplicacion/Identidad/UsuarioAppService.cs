using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Identidad.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Identidad;

namespace SaborByte.Aplicacion.Identidad;

public class UsuarioAppService(IAppDbContext db, IPasswordHasher passwordHasher)
{
    public async Task<List<UsuarioDto>> ListarAsync(CancellationToken ct = default)
    {
        var usuarios = await db.Usuarios
            .Include(u => u.Roles).ThenInclude(r => r.Rol)
            .Include(u => u.SucursalesAsignadas)
            .ToListAsync(ct);

        return usuarios.Select(MapearDto).ToList();
    }

    public async Task<Guid> CrearAsync(CrearUsuarioRequestDto request, CancellationToken ct = default)
    {
        var existe = await db.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario, ct);
        if (existe)
            throw new InvalidOperationException("Ya existe un usuario con ese nombre de usuario.");

        var roles = await db.Roles.Where(r => request.Roles.Contains(r.Nombre)).ToListAsync(ct);

        var usuario = new Usuario
        {
            NombreUsuario = request.NombreUsuario,
            Nombre = request.Nombre,
            Email = request.Email,
            HashPassword = passwordHasher.Hash(request.Password)
        };

        foreach (var rol in roles)
            usuario.Roles.Add(new UsuarioRol { Usuario = usuario, Rol = rol });

        foreach (var sucursalId in request.SucursalesAsignadas)
            usuario.SucursalesAsignadas.Add(new UsuarioSucursal { Usuario = usuario, SucursalId = sucursalId });

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        return usuario.Id;
    }

    public async Task DesactivarAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct)
            ?? throw new InvalidOperationException("El usuario no existe.");

        usuario.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    private static UsuarioDto MapearDto(Usuario u) => new()
    {
        Id = u.Id,
        NombreUsuario = u.NombreUsuario,
        Nombre = u.Nombre,
        Email = u.Email,
        Activo = u.Activo,
        Roles = u.Roles.Select(r => r.Rol!.Nombre).ToList(),
        SucursalesAsignadas = u.SucursalesAsignadas.Select(s => s.SucursalId).ToList()
    };
}
