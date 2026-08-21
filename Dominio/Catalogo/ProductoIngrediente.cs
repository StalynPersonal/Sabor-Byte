namespace SaborByte.Dominio.Catalogo;

// Receta / BOM: qué insumos (y en qué cantidad) consume un producto vendible.
public class ProductoIngrediente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public Guid InsumoId { get; set; }
    public Producto? Insumo { get; set; }
    public decimal CantidadUsada { get; set; }
    public bool IncluidoPorDefecto { get; set; } = true;
    public bool Opcional { get; set; }
}
