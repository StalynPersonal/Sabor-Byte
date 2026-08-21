namespace SaborByte.Dominio.Inventario;

public enum TipoMovimientoInventario
{
    Entrada,
    Salida,
    Ajuste,
    ConsumoVenta,
    ReversoCancelacion
}

public class MovimientoInventario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public Guid ProductoId { get; set; } // siempre un producto TipoProducto = Insumo

    public TipoMovimientoInventario Tipo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public decimal SaldoResultante { get; set; }

    // Referencia opcional a la operación que originó el movimiento (FacturaId, FacturaDetalleId...).
    public Guid? ReferenciaId { get; set; }
    public string? Nota { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public Guid? CreadoPorUsuarioId { get; set; }
}
