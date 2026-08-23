namespace SaborByte.Dominio.Identidad;

public class Permiso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Modulo { get; set; }
    public required string Accion { get; set; }
}

public class RolPermiso
{
    public Guid RolId { get; set; }
    public Rol? Rol { get; set; }
    public Guid PermisoId { get; set; }
    public Permiso? Permiso { get; set; }
}

public class UsuarioRol
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid RolId { get; set; }
    public Rol? Rol { get; set; }
}

public class UsuarioSucursal
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid SucursalId { get; set; }
    public Sucursales.Sucursal? Sucursal { get; set; }
}
