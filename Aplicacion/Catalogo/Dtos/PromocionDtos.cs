using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo.Dtos;

public class PromocionDto
{
    public Guid Id { get; set; }
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
    public required string Nombre { get; set; }
    public Guid? ProductoId { get; set; }
    public Guid? CategoriaId { get; set; }
    public TipoDescuentoPromocion TipoDescuento { get; set; }
    public decimal Valor { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; } = true;
}
