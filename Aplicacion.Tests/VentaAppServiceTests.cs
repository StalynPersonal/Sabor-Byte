using SaborByte.Aplicacion.Facturacion;
using SaborByte.Aplicacion.Facturacion.Dtos;
using SaborByte.Aplicacion.Identidad;
using SaborByte.Aplicacion.Identidad.Dtos;
using SaborByte.Aplicacion.Inventario;
using SaborByte.Aplicacion.Tests.Dobles;
using SaborByte.Dominio.Caja;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Identidad;
using SaborByte.Dominio.Pedidos;
using SaborByte.Infraestructura.Persistencia;
using Xunit;

namespace SaborByte.Aplicacion.Tests;

public class VentaAppServiceTests
{
    private readonly SaborByteDbContext _db = PruebaDbContextFactory.Crear();
    private readonly Guid _sucursalId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();
    private Guid _turnoId;
    private Guid _productoId;

    private VentaAppService CrearServicio()
    {
        var inventario = new InventarioAppService(_db);
        var autorizacion = new AutorizacionAppService(_db, new PasswordHasherFalso(), new AuditoriaEnMemoria());
        return new VentaAppService(_db, inventario, autorizacion, new AuditoriaEnMemoria(), new NotificadorComandasEnMemoria());
    }

    private async Task PrepararEscenarioAsync()
    {
        var caja = new SaborByte.Dominio.Caja.Caja { SucursalId = _sucursalId, Numero = "01" };
        var turno = new TurnoCaja { CajaId = caja.Id, Caja = caja, UsuarioAperturaId = _usuarioId, MontoAperturaEfectivo = 0 };
        var producto = new Producto
        {
            SucursalId = _sucursalId,
            Nombre = "Hamburguesa",
            Precio = 250m,
            AplicaItbis = true,
            TipoProducto = TipoProducto.Vendible
        };

        _db.Cajas.Add(caja);
        _db.TurnosCaja.Add(turno);
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        _turnoId = turno.Id;
        _productoId = producto.Id;
    }

    [Fact]
    public async Task CrearVenta_CalculaSubtotalItbisYTotalCorrectamente()
    {
        await PrepararEscenarioAsync();
        var servicio = CrearServicio();

        var resultado = await servicio.CrearVentaAsync(_sucursalId, _usuarioId, new CrearVentaRequestDto
        {
            TurnoCajaId = _turnoId,
            FormaPago = FormaPago.Efectivo,
            Items = [new ItemVentaDto { ProductoId = _productoId, Cantidad = 2 }]
        });

        Assert.Equal(500m, resultado.Subtotal);
        Assert.Equal(90m, resultado.Itbis); // 500 * 18%
        Assert.Equal(590m, resultado.Total);
    }

    [Fact]
    public async Task CrearVenta_ConPropinaPorcentual_SumaLaPropinaAlTotal()
    {
        await PrepararEscenarioAsync();
        var servicio = CrearServicio();

        var resultado = await servicio.CrearVentaAsync(_sucursalId, _usuarioId, new CrearVentaRequestDto
        {
            TurnoCajaId = _turnoId,
            FormaPago = FormaPago.Efectivo,
            PorcentajePropina = 10,
            Items = [new ItemVentaDto { ProductoId = _productoId, Cantidad = 1 }]
        });

        // 250 + 18% ITBIS = 295; propina 10% de 295 = 29.50; total = 324.50
        Assert.Equal(29.50m, resultado.Propina);
        Assert.Equal(324.50m, resultado.Total);
    }

    [Fact]
    public async Task CrearVenta_ConDescuentoSinAutorizacion_Rechaza()
    {
        await PrepararEscenarioAsync();
        var servicio = CrearServicio();

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.CrearVentaAsync(_sucursalId, _usuarioId,
            new CrearVentaRequestDto
            {
                TurnoCajaId = _turnoId,
                FormaPago = FormaPago.Efectivo,
                Items = [new ItemVentaDto { ProductoId = _productoId, Cantidad = 1, Descuento = 50 }]
            }));
    }

    [Fact]
    public async Task CrearVenta_ConCodigoDeAutorizacionValido_PermiteDescuento()
    {
        await PrepararEscenarioAsync();

        var passwordHasher = new PasswordHasherFalso();
        var rolSupervisor = new Rol { Nombre = "Supervisor" };
        var supervisor = new Usuario
        {
            NombreUsuario = "supervisor",
            Nombre = "Supervisor",
            HashPassword = passwordHasher.Hash("clave123")
        };
        _db.Roles.Add(rolSupervisor);
        _db.Usuarios.Add(supervisor);
        _db.UsuarioRoles.Add(new UsuarioRol { Usuario = supervisor, Rol = rolSupervisor });
        await _db.SaveChangesAsync();

        var autorizacion = new AutorizacionAppService(_db, passwordHasher, new AuditoriaEnMemoria());
        var autorizacionResultado = await autorizacion.SolicitarAsync(new SolicitarAutorizacionRequestDto
        {
            NombreUsuario = "supervisor",
            Password = "clave123",
            Accion = "Descuento"
        });

        var servicio = new VentaAppService(_db, new InventarioAppService(_db), autorizacion, new AuditoriaEnMemoria(), new NotificadorComandasEnMemoria());

        var resultado = await servicio.CrearVentaAsync(_sucursalId, _usuarioId, new CrearVentaRequestDto
        {
            TurnoCajaId = _turnoId,
            FormaPago = FormaPago.Efectivo,
            CodigoAutorizacionDescuento = autorizacionResultado.CodigoAutorizacion,
            Items = [new ItemVentaDto { ProductoId = _productoId, Cantidad = 1, Descuento = 50 }]
        });

        Assert.Equal(50m, resultado.Descuento);
    }

    [Fact]
    public async Task CrearVenta_TurnoCerrado_Rechaza()
    {
        await PrepararEscenarioAsync();
        var turno = await _db.TurnosCaja.FindAsync(_turnoId);
        turno!.Estado = EstadoTurnoCaja.Cerrado;
        await _db.SaveChangesAsync();

        var servicio = CrearServicio();

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.CrearVentaAsync(_sucursalId, _usuarioId,
            new CrearVentaRequestDto
            {
                TurnoCajaId = _turnoId,
                FormaPago = FormaPago.Efectivo,
                Items = [new ItemVentaDto { ProductoId = _productoId, Cantidad = 1 }]
            }));
    }

    // La regresión de concurrencia del NCF vive en VentaAppServiceConcurrenciaTests.cs (usa
    // SQLite en memoria, no el proveedor InMemory de aquí, porque InMemory no soporta
    // ExecuteUpdateAsync — que es justamente el mecanismo del fix).

    [Fact]
    public async Task CrearVenta_DesdeComanda_TomaLosItemsDeLaComandaYLaCierra()
    {
        await PrepararEscenarioAsync();

        var mesa = new Mesa { SucursalId = _sucursalId, Numero = "05" };
        var comanda = new Comanda { SucursalId = _sucursalId, MesaId = mesa.Id, Estado = EstadoComanda.EnviadaCocina };
        comanda.Items.Add(new ComandaItem
        {
            ComandaId = comanda.Id,
            ProductoId = _productoId,
            NombreProducto = "Hamburguesa",
            Cantidad = 2,
            PrecioUnitario = 250m,
            Estado = EstadoItemComanda.Listo
        });

        _db.Mesas.Add(mesa);
        _db.Comandas.Add(comanda);
        await _db.SaveChangesAsync();

        var servicio = CrearServicio();

        var resultado = await servicio.CrearVentaAsync(_sucursalId, _usuarioId, new CrearVentaRequestDto
        {
            TurnoCajaId = _turnoId,
            FormaPago = FormaPago.Efectivo,
            ComandaId = comanda.Id
        });

        Assert.Equal(500m, resultado.Subtotal); // 2 x 250, tomado de la comanda, no de Items

        var comandaActualizada = await _db.Comandas.FindAsync(comanda.Id);
        Assert.Equal(EstadoComanda.Cerrada, comandaActualizada!.Estado);

        var mesaActualizada = await _db.Mesas.FindAsync(mesa.Id);
        Assert.Equal(EstadoMesa.Libre, mesaActualizada!.Estado);
    }

    [Fact]
    public async Task CrearVenta_DesdeComandaConItemsEnRequest_Rechaza()
    {
        await PrepararEscenarioAsync();
        var servicio = CrearServicio();

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.CrearVentaAsync(_sucursalId, _usuarioId,
            new CrearVentaRequestDto
            {
                TurnoCajaId = _turnoId,
                FormaPago = FormaPago.Efectivo,
                ComandaId = Guid.NewGuid(),
                Items = [new ItemVentaDto { ProductoId = _productoId, Cantidad = 1 }]
            }));
    }
}
