using System.Security.Cryptography.X509Certificates;
using FacturacionElectronicaDGII.Firma;
using FacturacionElectronicaDGII.Modelos;
using FacturacionElectronicaDGII.Validacion;

namespace FacturacionElectronicaDGII;

// Implementación de referencia contra los XSD oficiales de DGII disponibles en el
// proyecto (carpeta /xsd). El envío real a los servicios web de DGII (EnviarADgiiAsync,
// ConsultarEstadoAsync) queda pendiente porque este proyecto no tiene acceso verificado
// a las URLs/contrato HTTP oficial de esos servicios — ver comentarios en cada método.
public class ServicioFacturacionElectronicaDgii(FacturacionElectronicaOpciones opciones) : IServicioFacturacionElectronica
{
    public ResultadoValidacion ValidarComprobante(ComprobanteDto comprobante)
        => ValidadorComprobante.Validar(comprobante);

    public string GenerarComprobanteXml(ComprobanteDto comprobante)
    {
        if (comprobante.TipoNcf != "32")
            throw new NotSupportedException(
                $"Solo se implementó la generación de XML para e-CF tipo 32 (Consumo). Tipo recibido: {comprobante.TipoNcf}.");

        return GeneradorXmlEcf32.Generar(comprobante);
    }

    public string GenerarCodigoSeguridad(ComprobanteDto comprobante)
        => GeneradorCodigoSeguridad.Generar(comprobante);

    public Task<string> FirmarComprobanteAsync(ComprobanteDto comprobante, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(opciones.RutaCertificado))
            throw new InvalidOperationException(
                "Falta configurar FacturacionElectronica:RutaCertificado (y PasswordCertificado) con el certificado digital homologado ante DGII.");

        var xml = GenerarComprobanteXml(comprobante);
        var certificado = X509CertificateLoader.LoadPkcs12FromFile(
            opciones.RutaCertificado, opciones.PasswordCertificado);

        var xmlFirmado = FirmadorXml.FirmarEnveloped(xml, certificado);
        return Task.FromResult(xmlFirmado);
    }

    public Task<ResultadoEnvioDgii> EnviarADgiiAsync(string comprobanteFirmado, CancellationToken ct = default)
        => throw new NotImplementedException(
            "Pendiente: se necesita la documentación técnica oficial de los servicios web de DGII " +
            "(URLs de ambiente de pruebas/producción, contrato SOAP/REST, manejo de la Semilla de autenticación) " +
            "para implementar el envío real. El XSD 'Semilla v.1.0.xsd' y 'ARECF v1.0.xsd' ya están modelados " +
            "como referencia de los mensajes de autenticación y acuse de recibo.");

    public Task<ResultadoEnvioDgii> ConsultarEstadoAsync(string trackId, CancellationToken ct = default)
        => throw new NotImplementedException(
            "Pendiente: requiere el endpoint oficial de consulta de estado de DGII (no disponible en este proyecto).");
}
