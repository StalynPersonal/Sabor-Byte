namespace SaborByte.Aplicacion.Caja.Dtos;

public class CajaResumenDto
{
    public Guid Id { get; set; }
    public required string Numero { get; set; }
    public bool Activa { get; set; }
    public long ProximoNumeroFactura { get; set; }
    public string? CodigoSucursal { get; set; }
}
