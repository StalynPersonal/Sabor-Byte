namespace SaborByte.Dominio.Catalogo;

// El producto (catálogo) es único para toda la empresa — lo que varía por sucursal es
// cuánto stock hay de él ahí. Un renglón por cada combinación (Producto Insumo, Sucursal),
// creado automáticamente al dar de alta el insumo (para cada sucursal existente) o al dar
// de alta una sucursal (para cada insumo existente), siempre arrancando en 0 — así nunca
// hace falta manejar el caso "no existe todavía el renglón de stock" en el resto del código.
public class StockSucursal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public Guid SucursalId { get; set; }

    public decimal StockActual { get; set; }
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
}
