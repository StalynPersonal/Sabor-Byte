using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Pedidos.Dtos;
using SaborByte.Dominio.Identidad;

namespace SaborByte.Aplicacion.Tests.Dobles;

// Dobles de prueba mínimos para las dependencias que normalmente implementa
// Infraestructura (hash real de password, SignalR, auditoría en BD) — en las
// pruebas de Aplicacion no nos interesa probar esos detalles de implementación.

public class PasswordHasherFalso : IPasswordHasher
{
    public string Hash(string password) => $"HASH:{password}";
    public bool Verificar(string hashAlmacenado, string passwordIngresada) => hashAlmacenado == $"HASH:{passwordIngresada}";
}

public class GeneradorTokenJwtFalso : IGeneradorTokenJwt
{
    public string Generar(Usuario usuario, IEnumerable<string> roles, IEnumerable<Guid> sucursalesPermitidas) => "token-falso";
}

public class NotificadorComandasEnMemoria : INotificadorComandas
{
    public List<string> EventosEmitidos { get; } = [];

    public Task ComandaCreadaAsync(Guid sucursalId, ComandaDto comanda)
    {
        EventosEmitidos.Add(nameof(ComandaCreadaAsync));
        return Task.CompletedTask;
    }

    public Task ItemComandaActualizadoAsync(Guid sucursalId, Guid comandaId, ComandaItemDto item)
    {
        EventosEmitidos.Add(nameof(ItemComandaActualizadoAsync));
        return Task.CompletedTask;
    }

    public Task ComandaListaParaEntregaAsync(Guid sucursalId, Guid comandaId)
    {
        EventosEmitidos.Add(nameof(ComandaListaParaEntregaAsync));
        return Task.CompletedTask;
    }

    public Task ComandaCerradaAsync(Guid sucursalId, Guid comandaId)
    {
        EventosEmitidos.Add(nameof(ComandaCerradaAsync));
        return Task.CompletedTask;
    }

    public Task ComandaCanceladaAsync(Guid sucursalId, Guid comandaId, Guid? comandaItemId)
    {
        EventosEmitidos.Add(nameof(ComandaCanceladaAsync));
        return Task.CompletedTask;
    }
}

public class AuditoriaEnMemoria : IAuditoriaService
{
    public List<(string Accion, string Entidad)> Registros { get; } = [];

    public Task RegistrarAsync(
        Guid? sucursalId, Guid usuarioId, string accion, string entidad,
        Guid? entidadId = null, string? detalle = null, CancellationToken ct = default)
    {
        Registros.Add((accion, entidad));
        return Task.CompletedTask;
    }
}
