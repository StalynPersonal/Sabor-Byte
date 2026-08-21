namespace SaborByte.Dominio.Facturacion;

public class FacturaDetalle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FacturaId { get; set; }
    public Factura? Factura { get; set; }

    public Guid ProductoId { get; set; }
    public required string NombreProducto { get; set; } // snapshot al momento de la venta
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }

    // Tasa realmente aplicada a esta línea al momento de la venta (snapshot — si el
    // producto cambia su tasa después, las facturas ya emitidas no deben cambiar). Null
    // = el producto no aplicaba ITBIS (exento), distinto de 0 (tasa 0% pero sí "aplica").
    public decimal? TasaItbis { get; set; }
    public decimal Itbis { get; set; }
    public decimal Total { get; set; }
}
