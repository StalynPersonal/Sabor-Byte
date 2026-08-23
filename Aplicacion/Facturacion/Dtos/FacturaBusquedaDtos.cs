namespace SaborByte.Aplicacion.Facturacion.Dtos;

public class FacturaResumenDto
{
    public Guid Id { get; set; }
    public string? NumeroFactura { get; set; }
    public string? NumeroNcf { get; set; }

    // Ya van implícitos en los primeros 4 dígitos de NumeroFactura, pero se exponen
    // aparte para no obligar al usuario a "leer" el número armado.
    public string? SucursalCodigo { get; set; }
    public string? CajaNumero { get; set; }

    public decimal Total { get; set; }
    public DateTime FechaEmision { get; set; }
}

public class FacturaLineaDetalleDto
{
    public string NombreProducto { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal TasaItbis { get; set; }
    public decimal Itbis { get; set; }
    public decimal Total { get; set; }
    public decimal CantidadAcreditada { get; set; }
}

public class FacturaPagoDetalleDto
{
    public string NombreMetodoPago { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? NumeroComprobante { get; set; }
}

// Vista completa de una factura ya emitida — para la pantalla de solo lectura
// "Facturas" en Central (el detalle que abre el icono de ojo).
public class FacturaDetalleCompletoDto
{
    public Guid Id { get; set; }
    public string? NumeroFactura { get; set; }
    public string? NumeroNcf { get; set; }
    public string? SucursalNombre { get; set; }
    public string? SucursalCodigo { get; set; }
    public string? CajaNumero { get; set; }
    public DateTime FechaEmision { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteRncOCedula { get; set; }
    public string? CajeroNombre { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Itbis { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }

    // Solo tienen valor si la sucursal factura e-CF real (ver Sucursal.EcfActivo);
    // para NCF tradicional o "sin NCF" quedan null.
    public string? CodigoSeguridadDgii { get; set; }

    public List<FacturaLineaDetalleDto> Lineas { get; set; } = [];
    public List<FacturaPagoDetalleDto> Pagos { get; set; } = [];
}
