namespace SaborByte.Dominio.Catalogo;

// Componente de un combo: qué otro producto Vendible (y cuántas unidades) incluye.
// A diferencia de ProductoIngrediente (receta -> insumos), esto vincula productos
// vendibles entre sí; el descuento de inventario real se resuelve expandiendo cada
// componente a su propia receta (ver InventarioAppService.DescontarPorComboAsync).
public class ComboItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ComboId { get; set; }
    public Producto? Combo { get; set; }
    public Guid ProductoIncluidoId { get; set; }
    public Producto? ProductoIncluido { get; set; }
    public decimal Cantidad { get; set; } = 1;
}
