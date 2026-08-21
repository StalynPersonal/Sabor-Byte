namespace SaborByte.Web.Api.Dtos;

public enum TipoMovimientoInventario
{
    Entrada,
    Salida,
    Ajuste,
    ConsumoVenta,
    ReversoCancelacion
}

public class RegistrarEntradaRequestDto
{
    public Guid ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public string? Nota { get; set; }
}

public class RegistrarAjusteRequestDto
{
    public Guid ProductoId { get; set; }
    public decimal NuevoStock { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class MovimientoInventarioDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public TipoMovimientoInventario Tipo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal SaldoResultante { get; set; }
    public string? Nota { get; set; }
    public DateTime FechaHora { get; set; }
}
