using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Identidad.Dtos;
using SaborByte.Aplicacion.Interfaces;

namespace SaborByte.Aplicacion.Identidad;

public class AutenticacionAppService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IGeneradorTokenJwt generadorToken)
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Roles).ThenInclude(r => r.Rol)
            .Include(u => u.SucursalesAsignadas)
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario && u.Activo, ct);

        if (usuario is null || !passwordHasher.Verificar(usuario.HashPassword, request.Password))
            return null;

        var roles = usuario.Roles.Select(r => r.Rol!.Nombre).ToList();
        var sucursales = usuario.SucursalesAsignadas.Select(s => s.SucursalId).ToList();

        var token = generadorToken.Generar(usuario, roles, sucursales);

        return new LoginResponseDto
        {
            Token = token,
            Nombre = usuario.Nombre,
            Roles = roles,
            SucursalesPermitidas = sucursales
        };
    }
}
