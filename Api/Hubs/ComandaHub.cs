using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SaborByte.Api.Extensiones;

namespace SaborByte.Api.Hubs;

[Authorize]
public class ComandaHub : Hub
{
    public static string GrupoSucursal(Guid sucursalId) => $"Sucursal-{sucursalId}";

    public override async Task OnConnectedAsync()
    {
        foreach (var sucursalId in Context.User!.ObtenerSucursalesPermitidas())
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoSucursal(sucursalId));

        await base.OnConnectedAsync();
    }
}
