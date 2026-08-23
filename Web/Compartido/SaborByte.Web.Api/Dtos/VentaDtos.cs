namespace SaborByte.Web.Api.Dtos;

public class ProductoResumenDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal Precio { get; set; }
    public decimal TasaItbis { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;

    // Precio que se muestra en las pantallas de venta (Caja/Mesero): el que carga el
    // cliente en el carrito/pedido es el base (sin ITBIS) — este es solo para mostrar.
    public decimal PrecioConItbis => Precio * (1 + TasaItbis);
}

public class ItemVentaDto
{
    public Guid ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public List<Guid> IngredientesExcluidosIds { get; set; } = [];
    public decimal Descuento { get; set; }
}

public class PagoVentaRequestDto
{
    public Guid MetodoPagoId { get; set; }
    public decimal Monto { get; set; }
    public string? NumeroComprobante { get; set; }
}

public class CrearVentaRequestDto
{
    public Guid TurnoCajaId { get; set; }
    public Guid? ClienteId { get; set; }
    public List<PagoVentaRequestDto> Pagos { get; set; } = [];
    public Guid? ComandaId { get; set; }
    public List<ItemVentaDto> Items { get; set; } = [];
    public decimal? PorcentajePropina { get; set; }
    public decimal? MontoPropinaFijo { get; set; }
    public Guid? CodigoAutorizacionDescuento { get; set; }
}

public class VentaResultadoDto
{
    public Guid FacturaId { get; set; }
    public string? NumeroFactura { get; set; }
    public string? NumeroNcf { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Itbis { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaEmision { get; set; }
    public List<PagoVentaRequestDto> Pagos { get; set; } = [];

    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteRncOCedula { get; set; }
    public string? CajeroNombre { get; set; }
    public string? CodigoSeguridadDgii { get; set; }

    public string? MensajeDgii { get; set; }
}
