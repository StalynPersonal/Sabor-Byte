using SaborByte.Dominio.Comun;

namespace SaborByte.Dominio.Catalogo;

public enum TipoProducto
{
    Insumo,
    Vendible
}

// Catálogo de TODA la empresa (una Empresa, varias Sucursales — ver Empresa.cs): un
// producto existe una sola vez, no se duplica por sucursal. Lo que sí varía por
// sucursal es el stock (ver StockSucursal), no el producto en sí.
public class Producto : EntidadBase
{
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }

    // Código propio del producto (ej. SKU interno) — obligatorio y único en toda la
    // empresa (el catálogo es compartido por todas las sucursales, ver comentario de
    // clase), distinto de los códigos de barra (que además pueden ser varios por
    // producto: presentaciones distintas, códigos viejos y nuevos, etc.).
    public required string Codigo { get; set; }
    public ICollection<CodigoBarraProducto> CodigosBarra { get; set; } = [];

    public decimal Precio { get; set; }
    public decimal CostoUnitario { get; set; } // para reportes de rentabilidad; 0 si no se conoce aún
    public Guid CategoriaId { get; set; }
    public bool Activo { get; set; } = true;

    // Todo producto lleva ITBIS: 0.18 (18%, general), 0.16 (16%, algunos bienes) o
    // 0 (0%, ej. exportaciones). No existe "exento" como estado aparte — un producto
    // sin impuesto real se marca con tasa 0%. Ver Informe Técnico e-CF, catálogo
    // IndicadorFacturacion.
    public decimal TasaItbis { get; set; } = 0.18m;

    public TipoProducto TipoProducto { get; set; }

    // Todo Insumo es inventariable por definición. Un Vendible normalmente NO lo es (su
    // stock es indirecto, vía receta) — pero un Vendible de reventa (ej. agua embotellada,
    // que se compra y se vende igual, sin preparación) sí puede marcarse Inventariable para
    // llevar su propio StockSucursal, igual que un Insumo. Mutuamente excluyente con Receta:
    // un Vendible-Inventariable no debe tener líneas de receta (ver ProductoAppService).
    public bool Inventariable { get; set; }

    public Guid UnidadMedidaId { get; set; }
    public UnidadMedida? UnidadMedida { get; set; }

    // Combo: producto Vendible cuyo "contenido" son otros productos Vendibles (ver
    // ComboItem) en vez de una receta de insumos. Precio suele ser menor a la suma
    // de sus componentes vendidos por separado.
    public bool EsCombo { get; set; }
    public ICollection<ComboItem> ComponentesCombo { get; set; } = [];

    public ICollection<ProductoIngrediente> Receta { get; set; } = [];

    // Solo aplica a productos de tipo Insumo — un renglón por sucursal, ver StockSucursal.cs.
    public ICollection<StockSucursal> StockPorSucursal { get; set; } = [];
}

public class CodigoBarraProducto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public required string CodigoBarra { get; set; }
}
