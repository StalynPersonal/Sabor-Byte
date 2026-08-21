using SaborByte.Aplicacion.Pedidos.Dtos;

namespace SaborByte.Aplicacion.Interfaces;

// Implementado en Api con SignalR (ver sección 1.4 del plan). Aplicacion no conoce
// el mecanismo de transporte, solo que "algo" debe notificarse tras cada cambio.
public interface INotificadorComandas
{
    Task ComandaCreadaAsync(Guid sucursalId, ComandaDto comanda);
    Task ItemComandaActualizadoAsync(Guid sucursalId, Guid comandaId, ComandaItemDto item);
    Task ComandaListaParaEntregaAsync(Guid sucursalId, Guid comandaId);
    Task ComandaCerradaAsync(Guid sucursalId, Guid comandaId);
    Task ComandaCanceladaAsync(Guid sucursalId, Guid comandaId, Guid? comandaItemId);
}
