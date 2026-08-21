using SaborByte.Dominio.Caja;

namespace SaborByte.Aplicacion.Facturacion.Dtos;

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

    // Propina: se acepta un % sugerido (ej. 10) o un monto fijo; si ambos vienen,
    // el monto fijo tiene prioridad. El reparto entre meseros queda fuera de v1.
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
