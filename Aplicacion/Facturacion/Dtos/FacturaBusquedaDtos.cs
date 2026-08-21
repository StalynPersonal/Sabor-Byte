namespace SaborByte.Aplicacion.Facturacion.Dtos;

public class FacturaResumenDto
{
    public Guid Id { get; set; }
    public string? NumeroNcf { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaEmision { get; set; }
}
