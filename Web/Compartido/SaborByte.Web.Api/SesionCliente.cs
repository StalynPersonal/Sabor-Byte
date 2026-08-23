namespace SaborByte.Web.Api;

public record SucursalPermitida(Guid Id, string Nombre, string? EmpresaNombre = null);

// Estado de sesión en memoria del navegador (vive mientras la pestaña esté abierta).
// v1 no persiste el token entre recargas de página; se puede añadir localStorage más adelante.
public class SesionCliente
{
    public string? Token { get; private set; }
    public string? Nombre { get; private set; }
    public List<string> Roles { get; private set; } = [];
    public List<SucursalPermitida> SucursalesPermitidas { get; private set; } = [];

    // Sucursal sobre la que el usuario está operando ahora mismo. Con una sola sucursal
    // asignada se autoselecciona; con varias, queda Guid.Empty hasta que el usuario la
    // elige explícitamente en /seleccionar-sucursal — antes de esto, ninguna pantalla debía
    // asumir en silencio "la primera sucursal de la lista" (ver RequiereSeleccionSucursal).
    public Guid SucursalActivaId { get; private set; }

    public bool EstaAutenticado => !string.IsNullOrEmpty(Token);
    public bool RequiereSeleccionSucursal => SucursalesPermitidas.Count > 1 && SucursalActivaId == Guid.Empty;

    public event Action? CambioSesion;

    public void EstablecerSesion(string token, string nombre, List<string> roles, List<SucursalPermitida> sucursales)
    {
        Token = token;
        Nombre = nombre;
        Roles = roles;
        SucursalesPermitidas = sucursales;
        SucursalActivaId = sucursales.Count == 1 ? sucursales[0].Id : Guid.Empty;
        CambioSesion?.Invoke();
    }

    public void SeleccionarSucursalActiva(Guid sucursalId)
    {
        if (!SucursalesPermitidas.Any(s => s.Id == sucursalId))
            throw new InvalidOperationException("Esa sucursal no está entre las asignadas al usuario.");

        SucursalActivaId = sucursalId;
        CambioSesion?.Invoke();
    }

    public void CerrarSesion()
    {
        Token = null;
        Nombre = null;
        Roles = [];
        SucursalesPermitidas = [];
        SucursalActivaId = Guid.Empty;
        CambioSesion?.Invoke();
    }
}
