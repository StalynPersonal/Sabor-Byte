using FacturacionElectronicaDGII.Modelos;
using FacturacionElectronicaDGII.Validacion;

namespace FacturacionElectronicaDGII;

public class ResultadoEnvioDgii
{
    public required string TrackId { get; set; }
    public required string Estado { get; set; } // Aceptado, Rechazado, Condicional, EnProceso
    public string? Mensaje { get; set; }
}

// Punto de entrada público de la librería. Un sistema anfitrión solo depende de esta interfaz.
public interface IServicioFacturacionElectronica
{
    ResultadoValidacion ValidarComprobante(ComprobanteDto comprobante);

    // Solo e-CF 32 (Consumo) implementado por ahora — ver GeneradorXmlEcf32.
    string GenerarComprobanteXml(ComprobanteDto comprobante);

    // Recibe el XML YA FIRMADO (no el comprobante sin firmar): el código de seguridad
    // no es un hash propio calculado sobre el comprobante, es literalmente derivado del
    // SignatureValue de la firma digital — ver GeneradorCodigoSeguridad.
    string GenerarCodigoSeguridad(string comprobanteFirmado);

    Task<string> FirmarComprobanteAsync(ComprobanteDto comprobante, CancellationToken ct = default);

    Task<ResultadoEnvioDgii> EnviarADgiiAsync(string comprobanteFirmado, CancellationToken ct = default);

    Task<ResultadoEnvioDgii> ConsultarEstadoAsync(string trackId, CancellationToken ct = default);
}
