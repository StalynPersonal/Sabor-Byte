namespace SaborByte.Web.Api.Dtos;

public class VentaSuspendidaResumenDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ClienteNombre { get; set; }
    public int CantidadItems { get; set; }
    public decimal Total { get; set; }
    public DateTime CreadoEn { get; set; }
    public string CreadoPorNombre { get; set; } = string.Empty;
}

public class VentaSuspendidaItemDto
{
    public Guid ProductoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal Cantidad { get; set; }
    public List<Guid> IngredientesExcluidosIds { get; set; } = [];
    public List<string> NombresExcluidos { get; set; } = [];
}

public class VentaSuspendidaDetalleDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public decimal PorcentajePropina { get; set; }
    public decimal MontoDescuento { get; set; }
    public bool DescuentoAutorizado { get; set; }
    public List<VentaSuspendidaItemDto> Items { get; set; } = [];
}

public class GuardarVentaSuspendidaRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public Guid? TurnoCajaId { get; set; }
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public decimal PorcentajePropina { get; set; }
    public decimal MontoDescuento { get; set; }
    public bool DescuentoAutorizado { get; set; }
    public List<VentaSuspendidaItemDto> Items { get; set; } = [];
}
