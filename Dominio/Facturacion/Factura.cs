namespace SaborByte.Dominio.Facturacion;

public enum EstadoDgii
{
    NoAplica, // EcfActivo = false para esta venta: NCF tradicional o sin NCF
    Pendiente,
    Aceptado,
    Rechazado,
    Contingencia
}

public class Factura
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public Guid CajaTurnoId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? ComandaId { get; set; }

    public string? NumeroNcf { get; set; }
    public string? TipoComprobante { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Itbis { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }

    public EstadoDgii EstadoDgii { get; set; } = EstadoDgii.NoAplica;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

    public Guid CreadoPorUsuarioId { get; set; }

    public ICollection<FacturaDetalle> Detalle { get; set; } = [];
}
