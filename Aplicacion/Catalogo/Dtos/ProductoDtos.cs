using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo.Dtos;

public class ProductoResumenDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public string? ImagenUrl { get; set; }
    public string? CodigoBarra { get; set; }
    public decimal Precio { get; set; }
    public bool AplicaItbis { get; set; }
    public TipoProducto TipoProducto { get; set; }
}

public class ComponenteComboRequestDto
{
    public Guid ProductoIncluidoId { get; set; }
    public decimal Cantidad { get; set; } = 1;
}

public class CrearComboRequestDto
{
    public required string Nombre { get; set; }
    public decimal Precio { get; set; }
    public Guid? CategoriaId { get; set; }
    public List<ComponenteComboRequestDto> Componentes { get; set; } = [];
}

public class ProductoDetalleDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string? CodigoBarra { get; set; }
    public decimal Precio { get; set; }
    public decimal? CostoUnitario { get; set; }
    public Guid? CategoriaId { get; set; }
    public bool Activo { get; set; }
    public bool AplicaItbis { get; set; }
    public decimal TasaItbis { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public string UnidadMedida { get; set; } = "Unidad";
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public decimal StockActual { get; set; }
    public bool EsCombo { get; set; }
}

public class GuardarProductoRequestDto
{
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string? CodigoBarra { get; set; }
    public decimal Precio { get; set; }
    public decimal? CostoUnitario { get; set; }
    public Guid? CategoriaId { get; set; }
    public bool AplicaItbis { get; set; } = true;
    public decimal TasaItbis { get; set; } = 0.18m;
    public TipoProducto TipoProducto { get; set; }
    public string UnidadMedida { get; set; } = "Unidad";
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }

    // Receta (BOM), solo aplica cuando TipoProducto = Vendible.
    public List<IngredienteRequestDto> Receta { get; set; } = [];
}

public class IngredienteRequestDto
{
    public Guid InsumoId { get; set; }
    public decimal CantidadUsada { get; set; }
    public bool IncluidoPorDefecto { get; set; } = true;
    public bool Opcional { get; set; }
}

public class CategoriaDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public int Orden { get; set; }
}

public class GuardarCategoriaRequestDto
{
    public required string Nombre { get; set; }
    public int Orden { get; set; }
}
