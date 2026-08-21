using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Identidad.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Identidad;

namespace SaborByte.Aplicacion.Identidad;

// Flujo de "supervisor override" descrito en la sección 7 del plan: un cajero
// solicita autorización, un Supervisor/Admin ingresa sus credenciales, y se emite
// un código de un solo uso que se adjunta a la operación sensible (ej. descuento).
public class AutorizacionAppService(IAppDbContext db, IPasswordHasher passwordHasher, IAuditoriaService auditoria)
{
    private static readonly string[] RolesAutorizan = ["Supervisor", "Admin"];

    public async Task<SolicitarAutorizacionResponseDto> SolicitarAsync(
        SolicitarAutorizacionRequestDto request, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Roles).ThenInclude(r => r.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario && u.Activo, ct);

        if (usuario is null || !passwordHasher.Verificar(usuario.HashPassword, request.Password))
            throw new InvalidOperationException("Credenciales de supervisor incorrectas.");

        var tieneRolAutorizante = usuario.Roles.Any(r => RolesAutorizan.Contains(r.Rol!.Nombre));
        if (!tieneRolAutorizante)
            throw new InvalidOperationException("El usuario no tiene rol de Supervisor o Admin para autorizar esta acción.");

        var autorizacion = new AutorizacionSupervisor
        {
            UsuarioAutorizanteId = usuario.Id,
            Accion = request.Accion
        };

        db.AutorizacionesSupervisor.Add(autorizacion);
        await db.SaveChangesAsync(ct);

        await auditoria.RegistrarAsync(null, usuario.Id, $"Autorizo:{request.Accion}", "AutorizacionSupervisor", autorizacion.Id, ct: ct);

        return new SolicitarAutorizacionResponseDto { CodigoAutorizacion = autorizacion.Id, Expira = autorizacion.Expira };
    }

    // Valida y marca el código como usado (no se puede reutilizar). Lanza si es inválido.
    public async Task ValidarYConsumirAsync(Guid codigo, string accion, CancellationToken ct = default)
    {
        var autorizacion = await db.AutorizacionesSupervisor.FirstOrDefaultAsync(a => a.Id == codigo, ct)
            ?? throw new InvalidOperationException("Código de autorización inválido.");

        if (autorizacion.Usada)
            throw new InvalidOperationException("El código de autorización ya fue utilizado.");

        if (autorizacion.Expira < DateTime.UtcNow)
            throw new InvalidOperationException("El código de autorización expiró.");

        if (autorizacion.Accion != accion)
            throw new InvalidOperationException("El código de autorización no corresponde a esta acción.");

        autorizacion.Usada = true;
        await db.SaveChangesAsync(ct);
    }
}
