using Microsoft.AspNetCore.SignalR;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Pedidos.Dtos;

namespace SaborByte.Api.Hubs;

public class NotificadorComandasSignalR(IHubContext<ComandaHub> hub) : INotificadorComandas
{
    public Task ComandaCreadaAsync(Guid sucursalId, ComandaDto comanda) =>
        hub.Clients.Group(ComandaHub.GrupoSucursal(sucursalId)).SendAsync("ComandaCreada", comanda);

    public Task ItemComandaActualizadoAsync(Guid sucursalId, Guid comandaId, ComandaItemDto item) =>
        hub.Clients.Group(ComandaHub.GrupoSucursal(sucursalId)).SendAsync("ItemComandaActualizado", comandaId, item);

    public Task ComandaListaParaEntregaAsync(Guid sucursalId, Guid comandaId) =>
        hub.Clients.Group(ComandaHub.GrupoSucursal(sucursalId)).SendAsync("ComandaListaParaEntrega", comandaId);

    public Task ComandaCerradaAsync(Guid sucursalId, Guid comandaId) =>
        hub.Clients.Group(ComandaHub.GrupoSucursal(sucursalId)).SendAsync("ComandaCerrada", comandaId);

    public Task ComandaCanceladaAsync(Guid sucursalId, Guid comandaId, Guid? comandaItemId) =>
        hub.Clients.Group(ComandaHub.GrupoSucursal(sucursalId)).SendAsync("ComandaCancelada", comandaId, comandaItemId);
}
