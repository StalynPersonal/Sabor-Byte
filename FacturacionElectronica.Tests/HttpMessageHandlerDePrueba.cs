using System.Net;

namespace FacturacionElectronicaDGII.Tests;

// Reemplaza las llamadas reales a DGII por respuestas fijas — así se puede probar el
// flujo completo (autenticación + envío + parseo de la respuesta) sin red ni cuenta de
// pruebas de DGII. Cada prueba decide qué responder según la URL solicitada.
public class HttpMessageHandlerDePrueba(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        responder(request);

    public static HttpResponseMessage Ok(string contenido, string tipoContenido = "application/json") => new(HttpStatusCode.OK)
    {
        Content = new StringContent(contenido, System.Text.Encoding.UTF8, tipoContenido)
    };
}
