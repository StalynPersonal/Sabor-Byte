using SaborByte.Aplicacion.Caja;
using SaborByte.Aplicacion.Caja.Dtos;
using SaborByte.Aplicacion.Tests.Dobles;
using SaborByte.Infraestructura.Persistencia;
using Xunit;

namespace SaborByte.Aplicacion.Tests;

public class CajaAppServiceTests
{
    private static async Task<(CajaAppService Servicio, SaborByteDbContext Db, Guid CajaId)> CrearEscenarioAsync()
    {
        var db = PruebaDbContextFactory.Crear();
        var caja = new SaborByte.Dominio.Caja.Caja { SucursalId = Guid.NewGuid(), Numero = "01" };
        db.Cajas.Add(caja);
        await db.SaveChangesAsync();

        var servicio = new CajaAppService(db, new AuditoriaEnMemoria());
        return (servicio, db, caja.Id);
    }

    [Fact]
    public async Task AbrirTurno_CajaSinTurnoAbierto_Permite()
    {
        var (servicio, _, cajaId) = await CrearEscenarioAsync();

        var turnoId = await servicio.AbrirTurnoAsync(Guid.NewGuid(),
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });

        Assert.NotEqual(Guid.Empty, turnoId);
    }

    [Fact]
    public async Task AbrirTurno_YaHayTurnoAbierto_Rechaza()
    {
        var (servicio, _, cajaId) = await CrearEscenarioAsync();
        await servicio.AbrirTurnoAsync(Guid.NewGuid(), new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.AbrirTurnoAsync(Guid.NewGuid(), new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 100 }));
    }

    [Fact]
    public async Task AbrirTurno_IpNoAutorizada_Rechaza()
    {
        var db = PruebaDbContextFactory.Crear();
        var caja = new SaborByte.Dominio.Caja.Caja { SucursalId = Guid.NewGuid(), Numero = "01", IpPermitida = "192.168.1.50" };
        db.Cajas.Add(caja);
        await db.SaveChangesAsync();
        var servicio = new CajaAppService(db, new AuditoriaEnMemoria());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.AbrirTurnoAsync(Guid.NewGuid(),
                new AbrirTurnoRequestDto { CajaId = caja.Id, MontoAperturaEfectivo = 500, IpOrigen = "10.0.0.9" }));
    }

    [Fact]
    public async Task CerrarTurno_TurnoAbierto_Cierra()
    {
        var (servicio, _, cajaId) = await CrearEscenarioAsync();
        var turnoId = await servicio.AbrirTurnoAsync(Guid.NewGuid(),
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });

        await servicio.CerrarTurnoAsync(Guid.NewGuid(), new CerrarTurnoRequestDto { TurnoCajaId = turnoId });

        var resumen = await servicio.ObtenerResumenAsync(turnoId);
        Assert.Equal(Dominio.Caja.EstadoTurnoCaja.Cerrado, resumen.Estado);
    }

    [Fact]
    public async Task CerrarTurno_YaCerrado_Rechaza()
    {
        var (servicio, _, cajaId) = await CrearEscenarioAsync();
        var turnoId = await servicio.AbrirTurnoAsync(Guid.NewGuid(),
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });
        await servicio.CerrarTurnoAsync(Guid.NewGuid(), new CerrarTurnoRequestDto { TurnoCajaId = turnoId });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.CerrarTurnoAsync(Guid.NewGuid(), new CerrarTurnoRequestDto { TurnoCajaId = turnoId }));
    }

    [Fact]
    public async Task AbrirTurno_TrasCerrarAnterior_Permite()
    {
        var (servicio, _, cajaId) = await CrearEscenarioAsync();
        var turno1 = await servicio.AbrirTurnoAsync(Guid.NewGuid(), new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 500 });
        await servicio.CerrarTurnoAsync(Guid.NewGuid(), new CerrarTurnoRequestDto { TurnoCajaId = turno1 });

        var turno2 = await servicio.AbrirTurnoAsync(Guid.NewGuid(), new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = 300 });

        Assert.NotEqual(turno1, turno2);
    }
}
