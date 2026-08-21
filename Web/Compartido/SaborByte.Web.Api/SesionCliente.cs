namespace SaborByte.Web.Api;

// Estado de sesión en memoria del navegador (vive mientras la pestaña esté abierta).
// v1 no persiste el token entre recargas de página; se puede añadir localStorage más adelante.
public class SesionCliente
{
    public string? Token { get; private set; }
    public string? Nombre { get; private set; }
    public List<string> Roles { get; private set; } = [];
    public List<Guid> SucursalesPermitidas { get; private set; } = [];

    public bool EstaAutenticado => !string.IsNullOrEmpty(Token);

    public event Action? CambioSesion;

    public void EstablecerSesion(string token, string nombre, List<string> roles, List<Guid> sucursales)
    {
        Token = token;
        Nombre = nombre;
        Roles = roles;
        SucursalesPermitidas = sucursales;
        CambioSesion?.Invoke();
    }

    public void CerrarSesion()
    {
        Token = null;
        Nombre = null;
        Roles = [];
        SucursalesPermitidas = [];
        CambioSesion?.Invoke();
    }
}
