namespace SaborByte.Aplicacion.CxcCxp.Dtos;

public class ProveedorDto
{
    public Guid Id { get; set; }
    public required string NombreORazonSocial { get; set; }
    public string? Rnc { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
}

public class GuardarProveedorRequestDto
{
    public required string NombreORazonSocial { get; set; }
    public string? Rnc { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
}
