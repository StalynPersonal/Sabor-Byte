using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using FacturacionElectronicaDGII.Dgii;
using FacturacionElectronicaDGII.Firma;
using FacturacionElectronicaDGII.Modelos;
using FacturacionElectronicaDGII.Validacion;

namespace FacturacionElectronicaDGII;

// Implementación de referencia contra los XSD oficiales de DGII disponibles en el
// proyecto (carpeta /xsd) y las URLs reales del ambiente de pruebas "TesteCF"
// confirmadas por el usuario (ver FacturacionElectronicaOpciones). El contrato exacto
// de las respuestas JSON de DGII (nombres de campo, catálogo de códigos de estado/error)
// NO está verificado contra una respuesta real todavía — el parseo de abajo es
// case-insensitive y tolerante a campos ausentes para minimizar el riesgo de romper
// ante pequeñas diferencias, pero debe confirmarse en la primera prueba real contra
// TesteCF.
public class ServicioFacturacionElectronicaDgii(FacturacionElectronicaOpciones opciones, HttpClient httpClient) : IServicioFacturacionElectronica
{
    private static readonly JsonSerializerOptions OpcionesJson = new() { PropertyNameCaseInsensitive = true };

    public ResultadoValidacion ValidarComprobante(ComprobanteDto comprobante)
        => ValidadorComprobante.Validar(comprobante);

    public string GenerarComprobanteXml(ComprobanteDto comprobante)
    {
        if (comprobante.TipoNcf != "32")
            throw new NotSupportedException(
                $"Solo se implementó la generación de XML para e-CF tipo 32 (Consumo). Tipo recibido: {comprobante.TipoNcf}.");

        return GeneradorXmlEcf32.Generar(comprobante);
    }

    public string GenerarCodigoSeguridad(string comprobanteFirmado)
        => GeneradorCodigoSeguridad.Generar(comprobanteFirmado);

    public Task<string> FirmarComprobanteAsync(ComprobanteDto comprobante, CancellationToken ct = default)
    {
        var certificado = CargarCertificado();
        var xml = GenerarComprobanteXml(comprobante);
        var xmlFirmado = FirmadorXml.FirmarEnveloped(xml, certificado);
        return Task.FromResult(xmlFirmado);
    }

    public async Task<ResultadoEnvioDgii> EnviarADgiiAsync(string comprobanteFirmado, CancellationToken ct = default)
    {
        var certificado = CargarCertificado();

        var autenticador = new AutenticadorDgii(httpClient);
        var token = await autenticador.ObtenerTokenAsync(
            opciones.UrlSemilla, opciones.UrlValidacionCertificado, certificado, ct);

        using var contenido = new MultipartFormDataContent();
        var parteXml = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(comprobanteFirmado));
        parteXml.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
        contenido.Add(parteXml, "xml", "ecfFirmado.xml");

        using var solicitud = new HttpRequestMessage(HttpMethod.Post, opciones.UrlRecepcionEcf) { Content = contenido };
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        solicitud.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var respuesta = await httpClient.SendAsync(solicitud, ct);
        var cuerpo = await respuesta.Content.ReadAsStringAsync(ct);

        if (!respuesta.IsSuccessStatusCode)
        {
            return new ResultadoEnvioDgii
            {
                TrackId = string.Empty,
                Estado = "Rechazado",
                Mensaje = $"DGII respondió {(int)respuesta.StatusCode} {respuesta.StatusCode}: {cuerpo}"
            };
        }

        var acuse = JsonSerializer.Deserialize<AcuseDeReciboJsonDto>(cuerpo, OpcionesJson);

        return new ResultadoEnvioDgii
        {
            TrackId = acuse?.TrackId ?? string.Empty,
            // Estado 0 = recibido correctamente (ver ArecfDto); el estado FINAL
            // (Aceptado/Rechazado/Condicional) se obtiene recién vía ConsultarEstadoAsync.
            Estado = acuse?.Estado == 0 ? "EnProceso" : "Rechazado",
            Mensaje = acuse?.Mensaje ?? cuerpo
        };
    }

    public async Task<ResultadoEnvioDgii> ConsultarEstadoAsync(string trackId, CancellationToken ct = default)
    {
        var certificado = CargarCertificado();
        var autenticador = new AutenticadorDgii(httpClient);
        var token = await autenticador.ObtenerTokenAsync(
            opciones.UrlSemilla, opciones.UrlValidacionCertificado, certificado, ct);

        using var solicitud = new HttpRequestMessage(HttpMethod.Get, opciones.UrlConsultaTrackId + trackId);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        solicitud.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var respuesta = await httpClient.SendAsync(solicitud, ct);
        var cuerpo = await respuesta.Content.ReadAsStringAsync(ct);
        respuesta.EnsureSuccessStatusCode();

        var estado = JsonSerializer.Deserialize<ConsultaEstadoJsonDto>(cuerpo, OpcionesJson);

        return new ResultadoEnvioDgii
        {
            TrackId = trackId,
            Estado = estado?.Estado ?? "Desconocido",
            Mensaje = estado?.Mensaje ?? cuerpo
        };
    }

    private X509Certificate2 CargarCertificado()
    {
        if (string.IsNullOrWhiteSpace(opciones.RutaCertificado))
            throw new InvalidOperationException(
                "Falta configurar FacturacionElectronica:RutaCertificado (y PasswordCertificado) con el certificado digital homologado ante DGII.");

        return X509CertificateLoader.LoadPkcs12FromFile(opciones.RutaCertificado, opciones.PasswordCertificado);
    }

    // Nombres de campo tentativos (case-insensitive) — sin confirmar contra una
    // respuesta real de TesteCF todavía.
    private sealed class AcuseDeReciboJsonDto
    {
        public int? Estado { get; set; }
        public string? TrackId { get; set; }
        public string? Mensaje { get; set; }
    }

    private sealed class ConsultaEstadoJsonDto
    {
        public string? Estado { get; set; }
        public string? Mensaje { get; set; }
    }
}
