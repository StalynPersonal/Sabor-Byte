using System.Security.Claims;

namespace SaborByte.Api.Extensiones;

public static class ClaimsPrincipalExtensiones
{
    public static Guid ObtenerUsuarioId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier) ?? usuario.FindFirstValue("sub");
        return Guid.TryParse(valor, out var id) ? id : throw new InvalidOperationException("Token sin identificador de usuario válido.");
    }

    public static IReadOnlyCollection<Guid> ObtenerSucursalesPermitidas(this ClaimsPrincipal usuario) =>
        usuario.FindAll("sucursal")
            .Select(c => Guid.TryParse(c.Value, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

    public static bool TieneAccesoASucursal(this ClaimsPrincipal usuario, Guid sucursalId) =>
        usuario.ObtenerSucursalesPermitidas().Contains(sucursalId);

    public static Guid ObtenerSesionActivaId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue("sesion");
        return Guid.TryParse(valor, out var id) ? id : throw new InvalidOperationException("Token sin sesión activa válida.");
    }
}
