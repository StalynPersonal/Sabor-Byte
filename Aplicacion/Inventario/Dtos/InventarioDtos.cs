using SaborByte.Dominio.Inventario;

namespace SaborByte.Aplicacion.Inventario.Dtos;

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
    public required string Motivo { get; set; }
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
