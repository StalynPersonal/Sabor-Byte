using SaborByte.Dominio.Comun;

namespace SaborByte.Dominio.Catalogo;

public enum TipoProducto
{
    Insumo,
    Vendible
}

public class Producto : EntidadBase
{
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string? CodigoBarra { get; set; }
    public decimal Precio { get; set; }
    public decimal? CostoUnitario { get; set; } // para reportes de rentabilidad; opcional
    public Guid? CategoriaId { get; set; }
    public bool Activo { get; set; } = true;
    public bool AplicaItbis { get; set; } = true;

    // Tasa aplicada cuando AplicaItbis=true: 0.18 (18%, general), 0.16 (16%, algunos
    // bienes) o 0 (0%, ej. exportaciones — distinto de AplicaItbis=false/Exento).
    // Ver Informe Técnico e-CF, catálogo IndicadorFacturacion.
    public decimal TasaItbis { get; set; } = 0.18m;

    public TipoProducto TipoProducto { get; set; }
    public string UnidadMedida { get; set; } = "Unidad";

    // Combo: producto Vendible cuyo "contenido" son otros productos Vendibles (ver
    // ComboItem) en vez de una receta de insumos. Precio suele ser menor a la suma
    // de sus componentes vendidos por separado.
    public bool EsCombo { get; set; }
    public ICollection<ComboItem> ComponentesCombo { get; set; } = [];

    // Solo aplica a productos de tipo Insumo, controlan alertas de inventario.
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public decimal StockActual { get; set; }

    public ICollection<ProductoIngrediente> Receta { get; set; } = [];
}
