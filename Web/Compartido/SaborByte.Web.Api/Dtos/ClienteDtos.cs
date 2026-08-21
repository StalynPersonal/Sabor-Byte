namespace SaborByte.Web.Api.Dtos;

public enum TipoCliente
{
    Fiscal,
    Consumo
}

public class ClienteDto
{
    public Guid Id { get; set; }
    public string NombreORazonSocial { get; set; } = string.Empty;
    public string? RncOCedula { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public TipoCliente TipoCliente { get; set; }
    public bool Activo { get; set; }
}

public class GuardarClienteRequestDto
{
    public string NombreORazonSocial { get; set; } = string.Empty;
    public string? RncOCedula { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public TipoCliente TipoCliente { get; set; } = TipoCliente.Consumo;
}
