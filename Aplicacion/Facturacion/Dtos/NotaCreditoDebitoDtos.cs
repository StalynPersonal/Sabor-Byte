using SaborByte.Dominio.Facturacion;

namespace SaborByte.Aplicacion.Facturacion.Dtos;

public class CrearNotaRequestDto
{
    public Guid FacturaOriginalId { get; set; }
    public TipoNota Tipo { get; set; }
    public required string Motivo { get; set; }
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
