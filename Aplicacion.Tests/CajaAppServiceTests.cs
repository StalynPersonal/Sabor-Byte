using SaborByte.Aplicacion.Caja;
using SaborByte.Aplicacion.Caja.Dtos;
using SaborByte.Aplicacion.Tests.Dobles;
using SaborByte.Infraestructura.Persistencia;
using Xunit;

namespace SaborByte.Aplicacion.Tests;

public class CajaAppServiceTests
{
    private static async Task<(CajaAppService Servicio, SaborByteDbContext Db, Guid SucursalId, Guid CajaId)> CrearEscenarioAsync()
    {
        var db = PruebaDbContextFactory.Crear();
        var sucursalId = Guid.NewGuid();
        var caja = new SaborByte.Dominio.Caja.Caja { SucursalId = sucursalId, Numero = "01" };
        db.Cajas.Add(caja);
        await db.SaveChangesAsync();

        var servicio = new CajaAppService(db, new AuditoriaEnMemoria());
        return (servicio, db, sucursalId, caja.Id);
    }

    [Fact]
    public async Task AbrirTurno_CajaSinTurnoAbierto_Permite()
    {
        var (servicio, _, sucursalId, cajaId) = await CrearEscenarioAsync();

        var turnoId = await servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId],
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });

        Assert.NotEqual(Guid.Empty, turnoId);
    }

    [Fact]
    public async Task AbrirTurno_YaHayTurnoAbierto_Rechaza()
    {
        var (servicio, _, sucursalId, cajaId) = await CrearEscenarioAsync();
        await servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId], new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId], new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 100 }));
    }

    [Fact]
    public async Task AbrirTurno_IpNoAutorizada_Rechaza()
    {
        var db = PruebaDbContextFactory.Crear();
        var sucursalId = Guid.NewGuid();
        var caja = new SaborByte.Dominio.Caja.Caja { SucursalId = sucursalId, Numero = "01", IpPermitida = "192.168.1.50" };
        db.Cajas.Add(caja);
        await db.SaveChangesAsync();
        var servicio = new CajaAppService(db, new AuditoriaEnMemoria());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId],
                new AbrirTurnoRequestDto { CajaId = caja.Id, MontoAperturaEfectivo = 500, IpOrigen = "10.0.0.9" }));
    }

    // Regresión: antes del fix, cualquier usuario autenticado podía abrir/leer/cerrar
    // el turno de una caja de OTRA sucursal con solo conocer su GUID (IDOR).
    [Fact]
    public async Task AbrirTurno_CajaDeOtraSucursal_Rechaza()
    {
        var (servicio, _, _, cajaId) = await CrearEscenarioAsync();
        var sucursalDelAtacante = Guid.NewGuid(); // no tiene nada que ver con la caja

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalDelAtacante],
                new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 }));
    }

    [Fact]
    public async Task ObtenerResumen_TurnoDeOtraSucursal_Rechaza()
    {
        var (servicio, _, sucursalId, cajaId) = await CrearEscenarioAsync();
        var turnoId = await servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId],
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.ObtenerResumenAsync(turnoId, [Guid.NewGuid()]));
    }

    [Fact]
    public async Task CerrarTurno_TurnoDeOtraSucursal_Rechaza()
    {
        var (servicio, _, sucursalId, cajaId) = await CrearEscenarioAsync();
        var turnoId = await servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId],
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.CerrarTurnoAsync(Guid.NewGuid(), [Guid.NewGuid()], new CerrarTurnoRequestDto { TurnoCajaId = turnoId }));
    }

    [Fact]
    public async Task CerrarTurno_TurnoAbierto_Cierra()
    {
        var (servicio, _, sucursalId, cajaId) = await CrearEscenarioAsync();
        var turnoId = await servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId],
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });

        await servicio.CerrarTurnoAsync(Guid.NewGuid(), [sucursalId], new CerrarTurnoRequestDto { TurnoCajaId = turnoId });

        var resumen = await servicio.ObtenerResumenAsync(turnoId, [sucursalId]);
        Assert.Equal(Dominio.Caja.EstadoTurnoCaja.Cerrado, resumen.Estado);
    }

    [Fact]
    public async Task CerrarTurno_YaCerrado_Rechaza()
    {
        var (servicio, _, sucursalId, cajaId) = await CrearEscenarioAsync();
        var turnoId = await servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId],
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });
        await servicio.CerrarTurnoAsync(Guid.NewGuid(), [sucursalId], new CerrarTurnoRequestDto { TurnoCajaId = turnoId });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.CerrarTurnoAsync(Guid.NewGuid(), [sucursalId], new CerrarTurnoRequestDto { TurnoCajaId = turnoId }));
    }

    [Fact]
    public async Task AbrirTurno_TrasCerrarAnterior_Permite()
    {
        var (servicio, _, sucursalId, cajaId) = await CrearEscenarioAsync();
        var turno1 = await servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId], new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });
        await servicio.CerrarTurnoAsync(Guid.NewGuid(), [sucursalId], new CerrarTurnoRequestDto { TurnoCajaId = turno1 });

        var turno2 = await servicio.AbrirTurnoAsync(Guid.NewGuid(), [sucursalId], new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 300 });

        Assert.NotEqual(turno1, turno2);
    }
}
