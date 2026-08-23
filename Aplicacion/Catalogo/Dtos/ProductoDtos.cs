using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo.Dtos;

public class ProductoResumenDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public required string Codigo { get; set; }
    public string? ImagenUrl { get; set; }
    public decimal Precio { get; set; }
    public decimal TasaItbis { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public Guid CategoriaId { get; set; }
    public required string CategoriaNombre { get; set; }
}

public class ComponenteComboRequestDto
{
    public Guid ProductoIncluidoId { get; set; }
    public decimal Cantidad { get; set; } = 1;
}

public class CrearComboRequestDto
{
    public required string Nombre { get; set; }
    public required string Codigo { get; set; }
    public decimal Precio { get; set; }
    public Guid CategoriaId { get; set; }
    public List<ComponenteComboRequestDto> Componentes { get; set; } = [];
}

public class ProductoDetalleDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
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

    // Solo se llena en ObtenerAsync (detalle de un producto), no en el listado paginado.
    public List<RecetaItemDto> Receta { get; set; } = [];

    public DateTime CreadoEn { get; set; }
    public string? CreadoPorNombre { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public string? ActualizadoPorNombre { get; set; }
}

public class RecetaItemDto
{
    public Guid InsumoId { get; set; }
    public required string InsumoNombre { get; set; }
    public decimal CantidadUsada { get; set; }
    public bool IncluidoPorDefecto { get; set; }
    public bool Opcional { get; set; }
}

public class GuardarProductoRequestDto
{
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public required string Codigo { get; set; }
    public List<string> CodigosBarra { get; set; } = [];
    public decimal Precio { get; set; }
    public decimal? CostoUnitario { get; set; }
    public Guid CategoriaId { get; set; }
    public decimal TasaItbis { get; set; } = 0.18m;
    public TipoProducto TipoProducto { get; set; }

    // Solo relevante cuando TipoProducto = Vendible (un Insumo siempre es inventariable,
    // se ignora este valor). Mutuamente excluyente con Receta — ver ProductoAppService.
    public bool Inventariable { get; set; }

    public Guid UnidadMedidaId { get; set; }

    // Receta (BOM), solo aplica cuando TipoProducto = Vendible y no es Inventariable.
    public List<IngredienteRequestDto> Receta { get; set; } = [];
}

public class IngredienteRequestDto
{
    public Guid InsumoId { get; set; }
    public decimal CantidadUsada { get; set; } = 1;
    public bool IncluidoPorDefecto { get; set; } = true;
    public bool Opcional { get; set; }
}

public class CategoriaDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; }
}

public class GuardarCategoriaRequestDto
{
    public required string Nombre { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
}
