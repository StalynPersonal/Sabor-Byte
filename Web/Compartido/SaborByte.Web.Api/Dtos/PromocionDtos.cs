namespace SaborByte.Web.Api.Dtos;

public enum TipoDescuentoPromocion
{
    Porcentaje,
    MontoFijo
}

public class PromocionDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public Guid? SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid? ProductoId { get; set; }
    public string? ProductoNombre { get; set; }
    public Guid? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public TipoDescuentoPromocion TipoDescuento { get; set; }
    public decimal Valor { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; }
}

public class GuardarPromocionRequestDto
{
    public Guid? SucursalId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid? ProductoId { get; set; }
    public Guid? CategoriaId { get; set; }
    public TipoDescuentoPromocion TipoDescuento { get; set; }
    public decimal Valor { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; } = true;
}
