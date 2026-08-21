namespace SaborByte.Dominio.CxcCxp;

public enum EstadoCuenta
{
    Pendiente,
    PagadaParcial,
    Pagada,
    Vencida
}

public class CuentaPorCobrar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid FacturaId { get; set; }

    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoCuenta Estado { get; set; } = EstadoCuenta.Pendiente;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public ICollection<PagoCxC> Pagos { get; set; } = [];
}

public class PagoCxC
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CuentaPorCobrarId { get; set; }
    public CuentaPorCobrar? Cuenta { get; set; }

    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public required string FormaPago { get; set; }
}
