using SaborByte.Aplicacion.Inventario;
using SaborByte.Dominio.Catalogo;
using SaborByte.Infraestructura.Persistencia;
using Xunit;

namespace SaborByte.Aplicacion.Tests;

public class InventarioAppServiceTests
{
    private readonly SaborByteDbContext _db = PruebaDbContextFactory.Crear();
    private readonly Guid _sucursalId = Guid.NewGuid();

    [Fact]
    public async Task DescontarPorVenta_ProductoConReceta_DescuentaInsumo()
    {
        var insumo = new Producto { SucursalId = _sucursalId, Nombre = "Pan", TipoProducto = TipoProducto.Insumo, StockActual = 20 };
        var producto = new Producto { SucursalId = _sucursalId, Nombre = "Hamburguesa", TipoProducto = TipoProducto.Vendible };
        _db.Productos.AddRange(insumo, producto);
        _db.ProductoIngredientes.Add(new ProductoIngrediente { ProductoId = producto.Id, InsumoId = insumo.Id, CantidadUsada = 2 });
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);
        await servicio.DescontarPorVentaAsync(_sucursalId, producto.Id, 3, Guid.NewGuid(), [], null);

        var insumoActualizado = await _db.Productos.FindAsync(insumo.Id);
        Assert.Equal(14m, insumoActualizado!.StockActual); // 20 - (2 * 3)
    }

    [Fact]
    public async Task DescontarPorVenta_IngredienteExcluido_NoLoDescuenta()
    {
        var tomate = new Producto { SucursalId = _sucursalId, Nombre = "Tomate", TipoProducto = TipoProducto.Insumo, StockActual = 10 };
        var pan = new Producto { SucursalId = _sucursalId, Nombre = "Pan", TipoProducto = TipoProducto.Insumo, StockActual = 10 };
        var producto = new Producto { SucursalId = _sucursalId, Nombre = "Hamburguesa", TipoProducto = TipoProducto.Vendible };
        _db.Productos.AddRange(tomate, pan, producto);
        _db.ProductoIngredientes.Add(new ProductoIngrediente { ProductoId = producto.Id, InsumoId = tomate.Id, CantidadUsada = 1, Opcional = true });
        _db.ProductoIngredientes.Add(new ProductoIngrediente { ProductoId = producto.Id, InsumoId = pan.Id, CantidadUsada = 1 });
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);
        await servicio.DescontarPorVentaAsync(_sucursalId, producto.Id, 1, Guid.NewGuid(), [tomate.Id], null);

        Assert.Equal(10m, (await _db.Productos.FindAsync(tomate.Id))!.StockActual); // sin cambios
        Assert.Equal(9m, (await _db.Productos.FindAsync(pan.Id))!.StockActual); // descontado
    }

    [Fact]
    public async Task DescontarPorVenta_Combo_DescuentaLaRecetaDeCadaComponente()
    {
        var pan = new Producto { SucursalId = _sucursalId, Nombre = "Pan", TipoProducto = TipoProducto.Insumo, StockActual = 20 };
        var hamburguesa = new Producto { SucursalId = _sucursalId, Nombre = "Hamburguesa", TipoProducto = TipoProducto.Vendible };
        var refresco = new Producto { SucursalId = _sucursalId, Nombre = "Refresco", TipoProducto = TipoProducto.Vendible };
        var combo = new Producto { SucursalId = _sucursalId, Nombre = "Combo", TipoProducto = TipoProducto.Vendible, EsCombo = true };

        _db.Productos.AddRange(pan, hamburguesa, refresco, combo);
        _db.ProductoIngredientes.Add(new ProductoIngrediente { ProductoId = hamburguesa.Id, InsumoId = pan.Id, CantidadUsada = 1 });
        _db.ComboItems.Add(new ComboItem { ComboId = combo.Id, ProductoIncluidoId = hamburguesa.Id, Cantidad = 1 });
        _db.ComboItems.Add(new ComboItem { ComboId = combo.Id, ProductoIncluidoId = refresco.Id, Cantidad = 1 });
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);
        await servicio.DescontarPorVentaAsync(_sucursalId, combo.Id, 2, Guid.NewGuid(), [], null);

        // 2 combos -> 2 hamburguesas -> 2 panes descontados; el refresco no tiene receta propia, no cambia nada.
        Assert.Equal(18m, (await _db.Productos.FindAsync(pan.Id))!.StockActual);
    }

    [Fact]
    public async Task RevertirPorCancelacion_DevuelveElStock()
    {
        var insumo = new Producto { SucursalId = _sucursalId, Nombre = "Pan", TipoProducto = TipoProducto.Insumo, StockActual = 10 };
        var producto = new Producto { SucursalId = _sucursalId, Nombre = "Hamburguesa", TipoProducto = TipoProducto.Vendible };
        _db.Productos.AddRange(insumo, producto);
        _db.ProductoIngredientes.Add(new ProductoIngrediente { ProductoId = producto.Id, InsumoId = insumo.Id, CantidadUsada = 1 });
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);
        var referencia = Guid.NewGuid();
        await servicio.DescontarPorVentaAsync(_sucursalId, producto.Id, 4, referencia, [], null);
        Assert.Equal(6m, (await _db.Productos.FindAsync(insumo.Id))!.StockActual);

        await servicio.RevertirPorCancelacionAsync(_sucursalId, producto.Id, 4, referencia, [], null);
        Assert.Equal(10m, (await _db.Productos.FindAsync(insumo.Id))!.StockActual);
    }
}
