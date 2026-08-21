using SaborByte.Dominio.Clientes;

namespace SaborByte.Aplicacion.Clientes.Dtos;

public class ClienteDto
{
    public Guid Id { get; set; }
    public required string NombreORazonSocial { get; set; }
    public string? RncOCedula { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public TipoCliente TipoCliente { get; set; }
    public bool Activo { get; set; }
}

public class GuardarClienteRequestDto
{
    public required string NombreORazonSocial { get; set; }
    public string? RncOCedula { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public TipoCliente TipoCliente { get; set; } = TipoCliente.Consumo;
}
