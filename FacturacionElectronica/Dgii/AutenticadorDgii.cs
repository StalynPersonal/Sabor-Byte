using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using FacturacionElectronicaDGII.Firma;

namespace FacturacionElectronicaDGII.Dgii;

// Flujo de autenticación "Semilla" de DGII, confirmado contra
// Descripcion Tecnica Emisores Electronicos.pdf (págs. 7-9) y el ejemplo real
// xsd/certificado/semilla.xml / semilla_firmado.xml provisto por el usuario:
//
//   1. GET  {rutaSemilla}         -> XML <SemillaModel><valor/><fecha/></SemillaModel>
//   2. Firmar ese XML (mismo esquema XML-DSig enveloped ya usado para el e-CF)
//   3. POST {rutaValidacion}      -> multipart/form-data, campo "xml", el XML firmado
//   4. Respuesta JSON { token, expira, expedido } — el token es un JWT Bearer
//
// NOTA: {rutaSemilla}/{rutaValidacion} son las URLs COMPLETAS que debe proveer quien
// configure esto (FacturacionElectronicaOpciones.UrlSemilla / UrlValidacionCertificado).
// La documentación disponible en este proyecto (ver xsd/) no incluye las URLs reales
// de host de DGII (ese dato vive en un documento separado, "Descripción Técnica
// Servicios DGII", que aún no se ha conseguido) — sin esas URLs, este cliente no
// puede autenticarse contra el ambiente real todavía.
public class AutenticadorDgii(HttpClient httpClient)
{
    public async Task<string> ObtenerTokenAsync(string urlSemilla, string urlValidacionCertificado,
        X509Certificate2 certificado, CancellationToken ct = default)
    {
        var xmlSemilla = await httpClient.GetStringAsync(urlSemilla, ct);

        var semillaFirmada = FirmadorXml.FirmarEnveloped(xmlSemilla, certificado);

        using var contenido = new MultipartFormDataContent();
        var parteXml = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(semillaFirmada));
        parteXml.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");
        contenido.Add(parteXml, "xml", "semillaFirmada.xml");

        using var respuesta = await httpClient.PostAsync(urlValidacionCertificado, contenido, ct);
        respuesta.EnsureSuccessStatusCode();

        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaAutenticacionDgii>(cancellationToken: ct)
            ?? throw new InvalidOperationException("DGII no devolvió un token de autenticación válido.");

        return cuerpo.Token;
    }

    private sealed class RespuestaAutenticacionDgii
    {
        [JsonPropertyName("token")]
        public required string Token { get; set; }

        [JsonPropertyName("expira")]
        public DateTime? Expira { get; set; }
    }
}
