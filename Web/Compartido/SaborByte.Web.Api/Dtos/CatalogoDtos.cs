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
    public string? Codigo { get; set; }
    public List<string> CodigosBarra { get; set; } = [];
    public decimal Precio { get; set; }
    public decimal? CostoUnitario { get; set; }
    public Guid CategoriaId { get; set; }
    public bool Activo { get; set; }
    public decimal TasaItbis { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public bool Inventariable { get; set; }
    public Guid UnidadMedidaId { get; set; }
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public decimal StockActual { get; set; }
    public bool EsCombo { get; set; }
    public List<RecetaItemDto> Receta { get; set; } = [];
    public List<ComponenteComboDto> Componentes { get; set; } = [];

    public DateTime CreadoEn { get; set; }
    public string? CreadoPorNombre { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public string? ActualizadoPorNombre { get; set; }
}

public class RecetaItemDto
{
    public Guid InsumoId { get; set; }
    public string InsumoNombre { get; set; } = string.Empty;
    public decimal CantidadUsada { get; set; }
    public bool IncluidoPorDefecto { get; set; } = true;
    public bool Opcional { get; set; }
}

public class IngredienteRequestDto
{
    public Guid InsumoId { get; set; }
    public decimal CantidadUsada { get; set; } = 1;
    public bool IncluidoPorDefecto { get; set; } = true;
    public bool Opcional { get; set; }
}

public class GuardarProductoRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public List<string> CodigosBarra { get; set; } = [];
    public decimal Precio { get; set; }
    public decimal? CostoUnitario { get; set; }
    public Guid CategoriaId { get; set; }
    public decimal TasaItbis { get; set; } = 0.18m;
    public TipoProducto TipoProducto { get; set; }
    public bool Inventariable { get; set; }
    public Guid UnidadMedidaId { get; set; }
    public List<IngredienteRequestDto> Receta { get; set; } = [];
}

public class ComponenteComboRequestDto
{
    public Guid ProductoIncluidoId { get; set; }
    public decimal Cantidad { get; set; } = 1;
}

public class CrearComboRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public Guid CategoriaId { get; set; }
    public List<ComponenteComboRequestDto> Componentes { get; set; } = [];
}

public class ComponenteComboDto
{
    public Guid ProductoIncluidoId { get; set; }
    public string ProductoIncluidoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
}

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; }
}

public class GuardarCategoriaRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
}

public class UnidadMedidaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class GuardarUnidadMedidaRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public class ConfigurarUmbralesRequestDto
{
    public Guid ProductoId { get; set; }
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
}
