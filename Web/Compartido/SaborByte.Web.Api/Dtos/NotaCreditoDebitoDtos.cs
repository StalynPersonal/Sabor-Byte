namespace SaborByte.Web.Api.Dtos;

public enum TipoNota
{
    Credito,
    Debito
}

public class FacturaResumenDto
{
    public Guid Id { get; set; }
    public string? NumeroNcf { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaEmision { get; set; }
}

public class CrearNotaRequestDto
{
    public Guid FacturaOriginalId { get; set; }
    public TipoNota Tipo { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

public class NotaCreditoDebitoDto
{
    public Guid Id { get; set; }
    public Guid FacturaOriginalId { get; set; }
    public TipoNota Tipo { get; set; }
    public string? NumeroNcf { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaEmision { get; set; }
}
