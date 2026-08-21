using SaborByte.Aplicacion.Inventario;
using SaborByte.Aplicacion.Pedidos;
using SaborByte.Aplicacion.Pedidos.Dtos;
using SaborByte.Aplicacion.Tests.Dobles;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Pedidos;
using SaborByte.Infraestructura.Persistencia;
using Xunit;

namespace SaborByte.Aplicacion.Tests;

public class ComandaAppServiceTests
{
    private readonly SaborByteDbContext _db = PruebaDbContextFactory.Crear();
    private readonly Guid _sucursalId = Guid.NewGuid();
    private readonly Guid _meseroId = Guid.NewGuid();
    private Guid _productoId;

    private ComandaAppService CrearServicio(NotificadorComandasEnMemoria? notificador = null) =>
        new(_db, new InventarioAppService(_db), notificador ?? new NotificadorComandasEnMemoria(), new AuditoriaEnMemoria());

    private async Task<Guid> PrepararProductoConRecetaAsync()
    {
        var insumo = new Producto
        {
            SucursalId = _sucursalId,
            Nombre = "Pan",
            TipoProducto = TipoProducto.Insumo,
            StockActual = 50
        };
        var producto = new Producto
        {
            SucursalId = _sucursalId,
            Nombre = "Hamburguesa",
            Precio = 250m,
            TipoProducto = TipoProducto.Vendible
        };
        producto.Receta.Add(new ProductoIngrediente { Producto = producto, Insumo = insumo, InsumoId = insumo.Id, CantidadUsada = 1 });

        _db.Productos.AddRange(insumo, producto);
        await _db.SaveChangesAsync();

        _productoId = producto.Id;
        return insumo.Id;
    }

    [Fact]
    public async Task CrearComanda_DescuentaInventarioYNotifica()
    {
        var insumoId = await PrepararProductoConRecetaAsync();
        var notificador = new NotificadorComandasEnMemoria();
        var servicio = CrearServicio(notificador);

        var comanda = await servicio.CrearComandaAsync(_sucursalId, _meseroId, new CrearComandaRequestDto
        {
            Items = [new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 3 }]
        });

        Assert.Equal(EstadoComanda.EnviadaCocina, comanda.Estado);
        Assert.Contains("ComandaCreadaAsync", notificador.EventosEmitidos);

        var insumo = await _db.Productos.FindAsync(insumoId);
        Assert.Equal(47m, insumo!.StockActual); // 50 - 3
    }

    [Fact]
    public async Task CambiarEstadoItem_TransicionValida_Permite()
    {
        await PrepararProductoConRecetaAsync();
        var servicio = CrearServicio();
        var comanda = await servicio.CrearComandaAsync(_sucursalId, _meseroId, new CrearComandaRequestDto
        {
            Items = [new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 1 }]
        });
        var itemId = comanda.Items[0].Id;

        var resultado = await servicio.CambiarEstadoItemAsync(_sucursalId, itemId, EstadoItemComanda.EnPreparacion);

        Assert.Equal(EstadoItemComanda.EnPreparacion, resultado.Estado);
    }

    [Fact]
    public async Task CambiarEstadoItem_TransicionInvalida_Rechaza()
    {
        await PrepararProductoConRecetaAsync();
        var servicio = CrearServicio();
        var comanda = await servicio.CrearComandaAsync(_sucursalId, _meseroId, new CrearComandaRequestDto
        {
            Items = [new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 1 }]
        });
        var itemId = comanda.Items[0].Id;

        // Pendiente -> Listo directo no está permitido (debe pasar por EnPreparacion).
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.CambiarEstadoItemAsync(_sucursalId, itemId, EstadoItemComanda.Listo));
    }

    [Fact]
    public async Task CancelarItem_RevierteInventarioYRegistraAuditoria()
    {
        var insumoId = await PrepararProductoConRecetaAsync();
        var servicio = CrearServicio();
        var comanda = await servicio.CrearComandaAsync(_sucursalId, _meseroId, new CrearComandaRequestDto
        {
            Items = [new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 2 }]
        });
        var itemId = comanda.Items[0].Id;

        await servicio.CancelarItemAsync(_sucursalId, itemId, _meseroId,
            new CancelarItemRequestDto { Motivo = "Error al pedir", Rol = RolQueCancelo.Mesero });

        var insumo = await _db.Productos.FindAsync(insumoId);
        Assert.Equal(50m, insumo!.StockActual); // vuelve a 50, se revirtió el descuento

        var item = await _db.ComandaItems.FindAsync(itemId);
        Assert.Equal(EstadoItemComanda.Cancelado, item!.Estado);
    }

    [Fact]
    public async Task CancelarItem_YaEntregado_Rechaza()
    {
        await PrepararProductoConRecetaAsync();
        var servicio = CrearServicio();
        var comanda = await servicio.CrearComandaAsync(_sucursalId, _meseroId, new CrearComandaRequestDto
        {
            Items = [new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 1 }]
        });
        var itemId = comanda.Items[0].Id;

        await servicio.CambiarEstadoItemAsync(_sucursalId, itemId, EstadoItemComanda.EnPreparacion);
        await servicio.CambiarEstadoItemAsync(_sucursalId, itemId, EstadoItemComanda.Listo);
        await servicio.CambiarEstadoItemAsync(_sucursalId, itemId, EstadoItemComanda.Entregado);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.CancelarItemAsync(_sucursalId, itemId, _meseroId,
                new CancelarItemRequestDto { Motivo = "Tarde", Rol = RolQueCancelo.Mesero }));
    }

    [Fact]
    public async Task CancelarComanda_RevierteInventarioDeTodosLosItemsYLiberaMesa()
    {
        var insumoId = await PrepararProductoConRecetaAsync();
        var servicio = CrearServicio();

        var mesa = new Mesa { SucursalId = _sucursalId, Numero = "07" };
        _db.Mesas.Add(mesa);
        await _db.SaveChangesAsync();

        var comanda = await servicio.CrearComandaAsync(_sucursalId, _meseroId, new CrearComandaRequestDto
        {
            MesaId = mesa.Id,
            Items =
            [
                new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 2 },
                new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 1 }
            ]
        });

        var mesaOcupada = await _db.Mesas.FindAsync(mesa.Id);
        Assert.Equal(EstadoMesa.Ocupada, mesaOcupada!.Estado);

        await servicio.CancelarComandaAsync(_sucursalId, comanda.Id, _meseroId,
            new CancelarComandaRequestDto { Motivo = "Cliente se fue", Rol = RolQueCancelo.Mesero });

        var comandaActualizada = await _db.Comandas.FindAsync(comanda.Id);
        Assert.Equal(EstadoComanda.Cancelada, comandaActualizada!.Estado);

        var insumo = await _db.Productos.FindAsync(insumoId);
        Assert.Equal(50m, insumo!.StockActual); // se revirtieron los 3 (2+1) consumidos

        var mesaLibre = await _db.Mesas.FindAsync(mesa.Id);
        Assert.Equal(EstadoMesa.Libre, mesaLibre!.Estado);
    }

    [Fact]
    public async Task CancelarComanda_ConItemYaEntregado_DejaLaComandaCerradaNoAcancelada()
    {
        await PrepararProductoConRecetaAsync();
        var servicio = CrearServicio();

        var comanda = await servicio.CrearComandaAsync(_sucursalId, _meseroId, new CrearComandaRequestDto
        {
            Items =
            [
                new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 1 },
                new ItemComandaRequestDto { ProductoId = _productoId, Cantidad = 1 }
            ]
        });

        var primerItemId = comanda.Items[0].Id;
        await servicio.CambiarEstadoItemAsync(_sucursalId, primerItemId, EstadoItemComanda.EnPreparacion);
        await servicio.CambiarEstadoItemAsync(_sucursalId, primerItemId, EstadoItemComanda.Listo);
        await servicio.CambiarEstadoItemAsync(_sucursalId, primerItemId, EstadoItemComanda.Entregado);

        await servicio.CancelarComandaAsync(_sucursalId, comanda.Id, _meseroId,
            new CancelarComandaRequestDto { Motivo = "El segundo plato ya no se quiere", Rol = RolQueCancelo.Mesero });

        var comandaActualizada = await _db.Comandas.FindAsync(comanda.Id);
        // No puede quedar "Cancelada" del todo porque ya se entregó (y probablemente facturó) un ítem.
        Assert.Equal(EstadoComanda.Cerrada, comandaActualizada!.Estado);

        var segundoItem = await _db.ComandaItems.FindAsync(comanda.Items[1].Id);
        Assert.Equal(EstadoItemComanda.Cancelado, segundoItem!.Estado);
    }
}
