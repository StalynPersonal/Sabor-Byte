using Microsoft.AspNetCore.SignalR.Client;
using SaborByte.Web.Api.Dtos;

namespace SaborByte.Web.Api;

// Envoltorio del hub SignalR de comandas (ver sección 1.4 del plan). Se reconecta solo;
// si la conexión se cae, la app sigue usando HTTP normal — SignalR es solo un acelerador de UX.
public class ConexionComandas(SesionCliente sesion, string apiBaseUrl) : IAsyncDisposable
{
    private HubConnection? _conexion;

    public event Action<ComandaDto>? ComandaCreada;
    public event Action<Guid, ComandaItemDto>? ItemActualizado;
    public event Action<Guid>? ComandaLista;
    public event Action<Guid>? ComandaCerrada;
    public event Action<Guid, Guid?>? ComandaCancelada;

    public async Task ConectarAsync()
    {
        if (_conexion is not null || sesion.Token is null)
            return;

        _conexion = new HubConnectionBuilder()
            .WithUrl($"{apiBaseUrl}hubs/comandas", opciones =>
            {
                opciones.AccessTokenProvider = () => Task.FromResult<string?>(sesion.Token);
            })
            .WithAutomaticReconnect()
            .Build();

        _conexion.On<ComandaDto>("ComandaCreada", c => ComandaCreada?.Invoke(c));
        _conexion.On<Guid, ComandaItemDto>("ItemComandaActualizado", (comandaId, item) => ItemActualizado?.Invoke(comandaId, item));
        _conexion.On<Guid>("ComandaListaParaEntrega", id => ComandaLista?.Invoke(id));
        _conexion.On<Guid>("ComandaCerrada", id => ComandaCerrada?.Invoke(id));
        _conexion.On<Guid, Guid?>("ComandaCancelada", (comandaId, itemId) => ComandaCancelada?.Invoke(comandaId, itemId));

        await _conexion.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_conexion is not null)
            await _conexion.DisposeAsync();
    }
}
