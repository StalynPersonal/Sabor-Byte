using System.Net;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace SaborByte.Web.Api;

// Intercepta CUALQUIER respuesta 401 de la Api (token vencido, sesión cerrada del lado
// del servidor, etc.): cierra la sesión local y manda al login, en vez de dejar que cada
// método de SaborByteApiClient reviente con un JsonException al intentar deserializar
// el cuerpo de un 401 como si fuera la respuesta normal esperada (bug real: un reporte
// tardío con la sesión ya vencida tumbaba toda la pantalla con una excepción no
// controlada en vez de simplemente devolver al usuario al login).
public class SesionExpiradaHandler(SesionCliente sesion, NavigationManager navegacion) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var respuesta = await base.SendAsync(request, cancellationToken);

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Cerrar sesión y navegar solo hace falta la primera vez: si dos pantallas
            // dispararon llamadas en paralelo (o el usuario alcanzó a hacer clic en algo
            // más antes de que la redirección terminara), el primer 401 en llegar ya puso
            // Token en null — para ESE momento sesion.EstaAutenticado ya es false, así que
            // sin este "if" separado, el segundo 401 (y cualquier otro después) pasaba de
            // largo sin neutralizar el cuerpo y tumbaba la pantalla con un JsonException.
            if (sesion.EstaAutenticado)
            {
                sesion.CerrarSesion();
                navegacion.NavigateTo("/login");
            }

            // Sustituye el cuerpo por "null" y el código por 200 en TODO 401, sin importar
            // el estado de la sesión: así los métodos de SaborByteApiClient (que ya manejan
            // "?? []" / "?? new Dto()") siempre reciben un valor vacío en vez de una
            // excepción de deserialización.
            respuesta.Content = new StringContent("null", Encoding.UTF8, "application/json");
            respuesta.StatusCode = HttpStatusCode.OK;
        }

        return respuesta;
    }
}
