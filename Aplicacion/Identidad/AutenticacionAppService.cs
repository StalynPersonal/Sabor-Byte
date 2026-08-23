using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Identidad.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Identidad;

namespace SaborByte.Aplicacion.Identidad;

public class AutenticacionAppService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IGeneradorTokenJwt generadorToken)
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, string? ipOrigen, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Roles).ThenInclude(r => r.Rol)
            .Include(u => u.SucursalesAsignadas).ThenInclude(s => s.Sucursal).ThenInclude(s => s!.Empresa)
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario && u.Activo, ct);

        if (usuario is null || !passwordHasher.Verificar(usuario.HashPassword, request.Password))
            return null;

        var roles = usuario.Roles.Select(r => r.Rol!.Nombre).ToList();
        var sucursalesAsignadas = usuario.SucursalesAsignadas.ToList();
        var sucursalIds = sucursalesAsignadas.Select(s => s.SucursalId).ToList();

        // Registro de presencia (ver SesionActiva): no hay estado de sesión en el JWT en sí,
        // así que sin esto no habría forma de saber "quién está activo ahora y dónde".
        var sesionActiva = new SesionActiva
        {
            UsuarioId = usuario.Id,
            SucursalId = sucursalIds.Count == 1 ? sucursalIds[0] : null,
            IpOrigen = ipOrigen
        };
        db.SesionesActivas.Add(sesionActiva);
        await db.SaveChangesAsync(ct);

        var token = generadorToken.Generar(usuario, roles, sucursalIds, sesionActiva.Id);

        return new LoginResponseDto
        {
            Token = token,
            Nombre = usuario.Nombre,
            Roles = roles,
            SucursalesPermitidas = sucursalesAsignadas
                .Select(s => new SucursalPermitidaDto
                {
                    Id = s.SucursalId,
                    Nombre = s.Sucursal?.Nombre ?? "",
                    EmpresaNombre = s.Sucursal?.Empresa?.Nombre
                })
                .ToList()
        };
    }

    public async Task SeleccionarSucursalActivaAsync(Guid sesionActivaId, Guid usuarioId, Guid sucursalId, CancellationToken ct = default)
    {
        var sesion = await db.SesionesActivas.FirstOrDefaultAsync(s => s.Id == sesionActivaId && s.UsuarioId == usuarioId, ct)
            ?? throw new InvalidOperationException("La sesión no existe.");

        var tieneAcceso = await db.Usuarios
            .Where(u => u.Id == usuarioId)
            .SelectMany(u => u.SucursalesAsignadas)
            .AnyAsync(s => s.SucursalId == sucursalId, ct);

        if (!tieneAcceso)
            throw new InvalidOperationException("Esa sucursal no está asignada a este usuario.");

        sesion.SucursalId = sucursalId;
        sesion.FechaUltimaActividad = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task CerrarSesionAsync(Guid sesionActivaId, Guid usuarioId, CancellationToken ct = default)
    {
        var sesion = await db.SesionesActivas.FirstOrDefaultAsync(s => s.Id == sesionActivaId && s.UsuarioId == usuarioId, ct);
        if (sesion is null || sesion.FechaCierre is not null)
            return;

        sesion.FechaCierre = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    // Heartbeat: se llama en cada request autenticado (ver MiddlewareActividadSesion) para
    // que FechaUltimaActividad refleje uso real, no solo el momento del login. Se filtra a
    // "más de 60s desde la última escritura" para no pegarle a la base en cada request.
    public async Task RegistrarActividadAsync(Guid sesionActivaId, CancellationToken ct = default)
    {
        var limite = DateTime.UtcNow.AddSeconds(-60);
        await db.SesionesActivas
            .Where(s => s.Id == sesionActivaId && s.FechaCierre == null && s.FechaUltimaActividad < limite)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.FechaUltimaActividad, DateTime.UtcNow), ct);
    }

    // "Activas ahora" = sin cerrar Y con actividad reciente (< 15 min); una pestaña abierta
    // y olvidada no debería figurar como que el usuario sigue trabajando.
    public async Task<List<SesionActivaDto>> ListarActivasAsync(IReadOnlyCollection<Guid> sucursalesPermitidas, CancellationToken ct = default)
    {
        var limite = DateTime.UtcNow.AddMinutes(-15);

        var sesiones = await db.SesionesActivas
            .Where(s => s.FechaCierre == null && s.FechaUltimaActividad >= limite &&
                        s.SucursalId != null && sucursalesPermitidas.Contains(s.SucursalId.Value))
            .Join(db.Usuarios, s => s.UsuarioId, u => u.Id, (s, u) => new { s, u })
            .OrderByDescending(x => x.s.FechaUltimaActividad)
            .ToListAsync(ct);

        var sucursalIds = sesiones.Select(x => x.s.SucursalId!.Value).Distinct().ToList();
        var nombresSucursales = await db.Sucursales
            .Where(s => sucursalIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Nombre, ct);

        return sesiones.Select(x => new SesionActivaDto
        {
            UsuarioId = x.u.Id,
            NombreUsuario = x.u.NombreUsuario,
            Nombre = x.u.Nombre,
            SucursalId = x.s.SucursalId,
            SucursalNombre = x.s.SucursalId is Guid sid && nombresSucursales.TryGetValue(sid, out var n) ? n : null,
            FechaInicio = x.s.FechaInicio,
            FechaUltimaActividad = x.s.FechaUltimaActividad
        }).ToList();
    }
}
