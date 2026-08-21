namespace SaborByte.Web.Api.Dtos;

public enum TipoProducto
{
    Insumo,
    Vendible
}

public class ProductoDetalleDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string? CodigoBarra { get; set; }
    public decimal Precio { get; set; }
    public decimal? CostoUnitario { get; set; }
    public Guid? CategoriaId { get; set; }
    public bool Activo { get; set; }
    public bool AplicaItbis { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public string UnidadMedida { get; set; } = "Unidad";
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public decimal StockActual { get; set; }
    public bool EsCombo { get; set; }
}

public class IngredienteRequestDto
{
    public Guid InsumoId { get; set; }
    public decimal CantidadUsada { get; set; }
    public bool IncluidoPorDefecto { get; set; } = true;
    public bool Opcional { get; set; }
}

public class GuardarProductoRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string? CodigoBarra { get; set; }
    public decimal Precio { get; set; }
    public decimal? CostoUnitario { get; set; }
    public Guid? CategoriaId { get; set; }
    public bool AplicaItbis { get; set; } = true;
    public TipoProducto TipoProducto { get; set; }
    public string UnidadMedida { get; set; } = "Unidad";
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public List<IngredienteRequestDto> Receta { get; set; } = [];
}

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
}

public class GuardarCategoriaRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
}
