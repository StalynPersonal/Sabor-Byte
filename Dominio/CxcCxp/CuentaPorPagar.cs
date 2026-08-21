namespace SaborByte.Dominio.CxcCxp;

public class CuentaPorPagar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public Guid ProveedorId { get; set; }
    public required string DocumentoReferencia { get; set; }

    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoCuenta Estado { get; set; } = EstadoCuenta.Pendiente;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public ICollection<PagoCxP> Pagos { get; set; } = [];
}

public class PagoCxP
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CuentaPorPagarId { get; set; }
    public CuentaPorPagar? Cuenta { get; set; }

    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public required string FormaPago { get; set; }
}
