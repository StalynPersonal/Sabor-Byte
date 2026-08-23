namespace SaborByte.Dominio.Comun;

// Usada solo por Producto y Categoria: catálogo de toda la empresa, NO por sucursal
// (el stock sí es por sucursal — ver StockSucursal). El resto de las entidades
// (Cliente, Caja, Mesa, Factura...) declaran su propio SucursalId, no esta base.
public abstract class EntidadBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // Obligatorio: toda entidad de catálogo sabe quién la creó (auditoría). El sistema
    // siempre tiene un usuario autenticado creando estas cosas, nunca es un proceso anónimo.
    public Guid CreadoPorUsuarioId { get; set; }

    public DateTime? ActualizadoEn { get; set; }
    public Guid? ActualizadoPorUsuarioId { get; set; }
}
