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
}

public class CuentaPorCobrarDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoCuenta Estado { get; set; }
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
    public required string DocumentoReferencia { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoCuenta Estado { get; set; }
}
