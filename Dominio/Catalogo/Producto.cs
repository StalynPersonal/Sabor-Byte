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
    public Guid? CategoriaId { get; set; }
    public bool Activo { get; set; } = true;
    public bool AplicaItbis { get; set; } = true;
    public TipoProducto TipoProducto { get; set; }
    public string UnidadMedida { get; set; } = "Unidad";

    // Solo aplica a productos de tipo Insumo, controlan alertas de inventario.
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public decimal StockActual { get; set; }

    public ICollection<ProductoIngrediente> Receta { get; set; } = [];
}
