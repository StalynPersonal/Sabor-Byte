namespace SaborByte.Web.Api.Dtos;

public enum EstadoCotizacion
{
    Pendiente,
    Aceptada,
    Rechazada
}

public class CotizacionResumenDto
{
    public Guid Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? DescripcionEvento { get; set; }
    public DateTime? FechaEvento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public EstadoCotizacion Estado { get; set; }
    public int CantidadItems { get; set; }
    public decimal Total { get; set; }
    public DateTime CreadoEn { get; set; }
    public string? CreadoPorNombre { get; set; }
    public bool YaCargadaEnCarrito { get; set; }
}

public class CotizacionItemDto
{
    public Guid ProductoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal TasaItbis { get; set; }
    public decimal Cantidad { get; set; }
}

public class CotizacionDetalleDto
{
    public Guid Id { get; set; }
    public Guid? ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteTelefono { get; set; }
    public string? DescripcionEvento { get; set; }
    public DateTime? FechaEvento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Notas { get; set; }
    public EstadoCotizacion Estado { get; set; }
    public decimal PorcentajePropina { get; set; }
    public decimal MontoDescuento { get; set; }
    public DateTime CreadoEn { get; set; }
    public string? CreadoPorNombre { get; set; }
    public bool YaCargadaEnCarrito { get; set; }
    public List<CotizacionItemDto> Items { get; set; } = [];
}

public class GuardarCotizacionRequestDto
{
    public Guid? ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteTelefono { get; set; }
    public string? DescripcionEvento { get; set; }
    public DateTime? FechaEvento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Notas { get; set; }
    public decimal PorcentajePropina { get; set; }
    public decimal MontoDescuento { get; set; }
    public List<ItemCotizacionRequestDto> Items { get; set; } = [];
}

public class ItemCotizacionRequestDto
{
    public Guid ProductoId { get; set; }
    public decimal Cantidad { get; set; }
}

public class CargaCarritoCotizacionDto
{
    public Guid? ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal PorcentajePropina { get; set; }
    public decimal MontoDescuento { get; set; }
    public List<CotizacionItemDto> Items { get; set; } = [];
}
