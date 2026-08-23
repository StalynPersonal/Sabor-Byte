namespace SaborByte.Web.Api.Dtos;

public enum EstadoCuenta
{
    Pendiente,
    PagadaParcial,
    Pagada,
    Vencida
}

public class ProveedorDto
{
    public Guid Id { get; set; }
    public string NombreORazonSocial { get; set; } = string.Empty;
    public string? Rnc { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
}

public class GuardarProveedorRequestDto
{
    public string NombreORazonSocial { get; set; } = string.Empty;
    public string? Rnc { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
}

public class CrearCuentaPorCobrarRequestDto
{
    public Guid ClienteId { get; set; }
    public Guid FacturaId { get; set; }
    public decimal MontoOriginal { get; set; }
    public DateTime FechaVencimiento { get; set; }
}

public class CuentaPorCobrarDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoCuenta Estado { get; set; }
}

public class CrearCuentaPorPagarRequestDto
{
    public Guid ProveedorId { get; set; }
    public string DocumentoReferencia { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public DateTime FechaVencimiento { get; set; }
}

public class CuentaPorPagarDto
{
    public Guid Id { get; set; }
    public Guid ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public string DocumentoReferencia { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoCuenta Estado { get; set; }
}

public class RegistrarPagoRequestDto
{
    public decimal Monto { get; set; }
    public Guid MetodoPagoId { get; set; }
}

public class PagoCuentaDto
{
    public Guid Id { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPagoNombre { get; set; } = string.Empty;
}
