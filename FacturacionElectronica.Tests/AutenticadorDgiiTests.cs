using FacturacionElectronicaDGII.Dgii;

namespace FacturacionElectronicaDGII.Tests;

public class AutenticadorDgiiTests
{
    private const string XmlSemillaDeEjemplo = "<SemillaModel><valor>abc123</valor><fecha>2026-03-05</fecha></SemillaModel>";

    [Fact]
    public async Task Obtiene_el_token_firmando_la_semilla_y_leyendolo_de_la_respuesta()
    {
        var certificado = CertificadoDePrueba.GenerarConClavePrivada();
        string? xmlEnviadoAValidacion = null;

        var handler = new HttpMessageHandlerDePrueba(async request =>
        {
            if (request.Method == HttpMethod.Get)
                return HttpMessageHandlerDePrueba.Ok(XmlSemillaDeEjemplo, "text/xml");

            // POST de validación: el cuerpo es multipart con el XML de la semilla ya firmado
            // — se lee crudo (boundaries incluidos) en vez de parsearlo, alcanza para
            // confirmar qué contenido viajó.
            xmlEnviadoAValidacion = await request.Content!.ReadAsStringAsync();

            return HttpMessageHandlerDePrueba.Ok("""{"token":"jwt-de-prueba","expira":"2026-03-05T23:59:59"}""");
        });

        using var httpClient = new HttpClient(handler);
        var autenticador = new AutenticadorDgii(httpClient);

        var token = await autenticador.ObtenerTokenAsync(
            "https://dgii.local/semilla", "https://dgii.local/validar", certificado);

        Assert.Equal("jwt-de-prueba", token);
        Assert.Contains("<SemillaModel>", xmlEnviadoAValidacion);
        Assert.Contains("Signature", xmlEnviadoAValidacion); // la semilla viajó firmada, no cruda
    }

    [Fact]
    public async Task Falla_si_dgii_no_devuelve_un_token_en_la_respuesta()
    {
        var certificado = CertificadoDePrueba.GenerarConClavePrivada();

        var handler = new HttpMessageHandlerDePrueba(request => Task.FromResult(
            request.Method == HttpMethod.Get
                ? HttpMessageHandlerDePrueba.Ok(XmlSemillaDeEjemplo, "text/xml")
                : HttpMessageHandlerDePrueba.Ok("{}")));

        using var httpClient = new HttpClient(handler);
        var autenticador = new AutenticadorDgii(httpClient);

        // "token" es required en el DTO de respuesta — sin él, la deserialización debe
        // fallar en vez de devolver un token nulo/silencioso.
        await Assert.ThrowsAnyAsync<Exception>(() =>
            autenticador.ObtenerTokenAsync("https://dgii.local/semilla", "https://dgii.local/validar", certificado));
    }
}
