namespace SaborByte.Dominio.Clientes;

public enum TipoCliente
{
    Fiscal,   // tiene RNC, puede recibir crédito fiscal (e-CF 31)
    Consumo   // persona física sin RNC
}

public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }

    public required string NombreORazonSocial { get; set; }
    public string? RncOCedula { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public TipoCliente TipoCliente { get; set; } = TipoCliente.Consumo;

    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    // Nullable: el "Cliente Contado" genérico de cada sucursal lo crea el sistema, no un usuario.
    public Guid? CreadoPorUsuarioId { get; set; }

    // Marca el cliente "Cliente Contado" genérico que usa cada sucursal cuando una venta
    // no identifica un cliente real (Factura.ClienteId es obligatorio) — distingue este
    // registro de uno que un admin haya creado a mano con el mismo nombre por coincidencia.
    public bool EsGenerico { get; set; }
}
