namespace SaborByte.Dominio.Facturacion;

public enum TipoNota
{
    Credito, // e-CF 34 — anula o ajusta a la baja una factura ya emitida
    Debito   // e-CF 33 — ajusta al alza una factura ya emitida
}

// Referencia siempre a la Factura original (nunca se borra un comprobante emitido —
// ver sección 4/9 del plan). Se usa tanto para anulaciones completas como ajustes.
public class NotaCreditoDebito
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public Guid FacturaOriginalId { get; set; }

    public TipoNota Tipo { get; set; }
    public required string Motivo { get; set; }
    public decimal Monto { get; set; }

    public string? NumeroNcf { get; set; }
    public string? TipoComprobante { get; set; } // "33" o "34"
    public EstadoDgii EstadoDgii { get; set; } = EstadoDgii.NoAplica;

    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public Guid CreadoPorUsuarioId { get; set; }
}
