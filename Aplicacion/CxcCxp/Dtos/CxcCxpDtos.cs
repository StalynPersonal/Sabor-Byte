using SaborByte.Dominio.CxcCxp;

namespace SaborByte.Aplicacion.CxcCxp.Dtos;

public class CrearCuentaPorCobrarRequestDto
{
    public Guid ClienteId { get; set; }
    public Guid FacturaId { get; set; }
    public decimal MontoOriginal { get; set; }
    public DateTime FechaVencimiento { get; set; }
}

public class RegistrarPagoRequestDto
{
    public decimal Monto { get; set; }
    public Guid MetodoPagoId { get; set; }
    public string? NumeroComprobante { get; set; }
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
    public string RegistradaPorNombre { get; set; } = string.Empty;
}

public class PagoCuentaDto
{
    public Guid Id { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPagoNombre { get; set; } = string.Empty;
    public string? NumeroComprobante { get; set; }
    public string RegistradoPorNombre { get; set; } = string.Empty;
    public bool Anulado { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? AnuladoPorNombre { get; set; }
    public string? MotivoAnulacion { get; set; }
}

public class AnularPagoRequestDto
{
    public required string Motivo { get; set; }
}

public class CrearCuentaPorPagarRequestDto
{
    public Guid ProveedorId { get; set; }
    public required string DocumentoReferencia { get; set; }
    public decimal MontoOriginal { get; set; }
    public DateTime FechaVencimiento { get; set; }
}

public class CuentaPorPagarDto
{
    public Guid Id { get; set; }
    public Guid ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public required string DocumentoReferencia { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoCuenta Estado { get; set; }
    public string RegistradaPorNombre { get; set; } = string.Empty;
}
