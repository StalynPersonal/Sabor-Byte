using FacturacionElectronicaDGII.Modelos;
using FacturacionElectronicaDGII.Validacion;

namespace FacturacionElectronicaDGII;

// Implementación de referencia. La firma digital (Firma/) y el envío real a los
// servicios web de DGII (Envio/) se completan en la Fase 4 del roadmap, una vez
// se disponga del certificado y el ambiente de pruebas/homologación conectados.
public class ServicioFacturacionElectronicaDgii : IServicioFacturacionElectronica
{
    public ResultadoValidacion ValidarComprobante(ComprobanteDto comprobante)
        => ValidadorComprobante.Validar(comprobante);

    public string GenerarCodigoSeguridad(ComprobanteDto comprobante)
        => throw new NotImplementedException("Se implementa en la Fase 4 según especificación de código de seguridad de DGII.");

    public Task<string> FirmarComprobanteAsync(ComprobanteDto comprobante, CancellationToken ct = default)
        => throw new NotImplementedException("Se implementa en la Fase 4 con el certificado digital homologado ante DGII.");

    public Task<ResultadoEnvioDgii> EnviarADgiiAsync(string comprobanteFirmado, CancellationToken ct = default)
        => throw new NotImplementedException("Se implementa en la Fase 4 contra el servicio web de DGII.");

    public Task<ResultadoEnvioDgii> ConsultarEstadoAsync(string trackId, CancellationToken ct = default)
        => throw new NotImplementedException("Se implementa en la Fase 4 contra el servicio web de DGII.");
}
