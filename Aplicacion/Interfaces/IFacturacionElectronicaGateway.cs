namespace SaborByte.Aplicacion.Interfaces;

public class ResultadoEmisionEcf
{
    public bool Exitoso { get; set; }
    public List<string> ErroresValidacion { get; set; } = [];
    public string? TrackId { get; set; }
    public string? EstadoDgii { get; set; }
    public string? MensajeDgii { get; set; }
    public string? CodigoSeguridadDgii { get; set; }
}

// Adaptador entre el dominio de Sabor Byte (Factura/FacturaDetalle) y la librería
// independiente FacturacionElectronicaDGII. Implementado en Infraestructura.
public interface IFacturacionElectronicaGateway
{
    Task<ResultadoEmisionEcf> EmitirAsync(Guid facturaId, CancellationToken ct = default);
}
