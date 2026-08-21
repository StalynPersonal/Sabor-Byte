namespace SaborByte.Web.Api.Dtos;

public class ProductoResumenDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public string? CodigoBarra { get; set; }
    public decimal Precio { get; set; }
    public bool AplicaItbis { get; set; }
}

public class ItemVentaDto
{
    public Guid ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public List<Guid> IngredientesExcluidosIds { get; set; } = [];
    public decimal Descuento { get; set; }
}

public class CrearVentaRequestDto
{
    public Guid TurnoCajaId { get; set; }
    public Guid? ClienteId { get; set; }
    public FormaPago FormaPago { get; set; }
    public List<ItemVentaDto> Items { get; set; } = [];
    public decimal? PorcentajePropina { get; set; }
    public decimal? MontoPropinaFijo { get; set; }
}

public class VentaResultadoDto
{
    public Guid FacturaId { get; set; }
    public string? NumeroNcf { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Itbis { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaEmision { get; set; }
}
