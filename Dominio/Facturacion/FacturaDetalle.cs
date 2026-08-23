namespace SaborByte.Dominio.Facturacion;

public class FacturaDetalle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FacturaId { get; set; }
    public Factura? Factura { get; set; }

    public Guid ProductoId { get; set; }

    // Snapshot del producto al momento de la venta — si el producto se edita o se
    // desactiva después, esta línea no debe cambiar de significado retroactivamente.
    public required string NombreProducto { get; set; }
    public required string Codigo { get; set; }
    public string UnidadMedida { get; set; } = "Unidad";

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }

    // Tasa realmente aplicada a esta línea al momento de la venta (snapshot — si el
    // producto cambia su tasa después, las facturas ya emitidas no deben cambiar).
    public decimal TasaItbis { get; set; }
    public decimal Itbis { get; set; }
    public decimal Total { get; set; }

    // Cuánto de esta línea ya se acreditó vía notas de crédito (NotaCreditoDetalle).
    // Cantidad - CantidadAcreditada = lo que todavía se puede devolver/acreditar.
    public decimal CantidadAcreditada { get; set; }
}
