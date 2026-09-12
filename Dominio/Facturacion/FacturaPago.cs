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

    // Lo aplicado a la venta (lo que cuenta como ingreso/movimiento de caja) — cuando hay
    // cambio de por medio, es menor a lo que el cliente entregó realmente (ver MontoRecibido).
    public decimal Monto { get; set; }

    // Solo se llena para pagos en efectivo: lo que el cliente entregó físicamente (ej. un
    // billete de 200 para una cuenta de 170, con 30 de cambio). Null en el resto de los
    // casos, donde es igual a Monto. Existe solo para poder reimprimir el recibo mostrando
    // lo que el cliente realmente pagó, no lo neto aplicado.
    public decimal? MontoRecibido { get; set; }

    // Solo aplica cuando MetodoPago.RequiereComprobante (ej. Tarjeta): número de
    // comprobante/autorización que emite el datáfono al procesar el cobro. Opcional.
    public string? NumeroComprobante { get; set; }
}
