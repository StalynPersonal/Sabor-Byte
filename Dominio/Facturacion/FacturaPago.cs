using SaborByte.Dominio.Catalogo;

namespace SaborByte.Dominio.Facturacion;

// Desglose de cómo se pagó la factura: una factura puede tener más de una forma de
// pago a la vez (ej. mitad efectivo, mitad tarjeta) — antes solo se guardaba un
// FormaPago suelto en MovimientoCaja, sin soportar pagos mixtos ni el número de
// comprobante de la tarjeta.
public class FacturaPago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FacturaId { get; set; }
    public Factura? Factura { get; set; }

    public Guid MetodoPagoId { get; set; }
    public MetodoPago? MetodoPago { get; set; }
    public decimal Monto { get; set; }

    // Solo aplica cuando MetodoPago.RequiereComprobante (ej. Tarjeta): número de
    // comprobante/autorización que emite el datáfono al procesar el cobro. Opcional.
    public string? NumeroComprobante { get; set; }
}
