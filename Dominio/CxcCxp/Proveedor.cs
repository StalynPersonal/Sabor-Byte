namespace SaborByte.Dominio.CxcCxp;

public class Proveedor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public required string NombreORazonSocial { get; set; }
    public string? Rnc { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
}
