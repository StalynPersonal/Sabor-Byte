namespace SaborByte.Dominio.Facturacion;

// Solo existen notas de CRÉDITO en el sistema (e-CF 33 — anula o ajusta a la baja una
// factura ya emitida); no hay notas de débito, así que no hace falta un campo Tipo.
// Referencia siempre a la Factura original (nunca se borra un comprobante emitido —
// ver sección 4/9 del plan). Se usa tanto para anulaciones completas como ajustes.
// El Monto ya no se captura suelto: se calcula sumando Detalle (una línea por cada
// FacturaDetalle que se está acreditando y cuánto de esa cantidad), así se sabe con
// precisión qué se devolvió y FacturaDetalle.CantidadAcreditada queda al día.
public class NotaCredito
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public Guid FacturaOriginalId { get; set; }

    // Número interno correlativo propio de la nota (no el NCF fiscal, que solo existe si
    // hay secuencia activa) — mismo propósito que Factura.NumeroFactura: identificar
    // cualquier nota aunque la sucursal no facture fiscalmente todavía.
    public string? NumeroNota { get; set; }

    // FK al catálogo (Admin puede agregar/desactivar motivos); Nombre queda snapshotado
    // aparte para que la nota siga siendo legible aunque el motivo se renombre o desactive
    // después.
    public Guid? MotivoId { get; set; }
    public required string Motivo { get; set; }

    public decimal Monto { get; set; }

    public string? NumeroNcf { get; set; }
    public string? TipoComprobante { get; set; } // siempre "33" (nota de crédito)
    public EstadoDgii EstadoDgii { get; set; } = EstadoDgii.NoAplica;

    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public Guid CreadoPorUsuarioId { get; set; }

    public ICollection<NotaCreditoDetalle> Detalle { get; set; } = [];
}

// Una línea = "de este ítem de la factura original, se está acreditando esta cantidad".
// Permite notas parciales (factura con Cantidad=2, se acredita solo 1) y saber, sumando
// FacturaDetalle.CantidadAcreditada, si una línea ya se devolvió por completo.
public class NotaCreditoDetalle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NotaCreditoId { get; set; }
    public NotaCredito? Nota { get; set; }

    public Guid FacturaDetalleId { get; set; }
    public FacturaDetalle? FacturaDetalle { get; set; }

    public decimal Cantidad { get; set; }
    public decimal Monto { get; set; }
}
