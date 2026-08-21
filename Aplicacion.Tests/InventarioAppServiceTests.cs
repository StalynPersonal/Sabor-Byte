using SaborByte.Aplicacion.Inventario;
using SaborByte.Aplicacion.Inventario.Dtos;
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

    [Fact]
    public async Task RegistrarEntrada_SumaAlStockYActualizaCostoUnitario()
    {
        var insumo = new Producto { SucursalId = _sucursalId, Nombre = "Pollo", TipoProducto = TipoProducto.Insumo, StockActual = 10, CostoUnitario = 50m };
        _db.Productos.Add(insumo);
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);
        var usuarioId = Guid.NewGuid();
        await servicio.RegistrarEntradaAsync(_sucursalId, usuarioId, new RegistrarEntradaRequestDto
        {
            ProductoId = insumo.Id,
            Cantidad = 25,
            CostoUnitario = 55m,
            Nota = "Compra a proveedor X"
        });

        var actualizado = await _db.Productos.FindAsync(insumo.Id);
        Assert.Equal(35m, actualizado!.StockActual); // 10 + 25
        Assert.Equal(55m, actualizado.CostoUnitario); // costo de referencia actualizado

        var movimiento = Assert.Single(_db.MovimientosInventario);
        Assert.Equal(Dominio.Inventario.TipoMovimientoInventario.Entrada, movimiento.Tipo);
        Assert.Equal(25m, movimiento.Cantidad);
        Assert.Equal(35m, movimiento.SaldoResultante);
    }

    [Fact]
    public async Task RegistrarEntrada_CantidadNegativaOCero_Rechaza()
    {
        var insumo = new Producto { SucursalId = _sucursalId, Nombre = "Pollo", TipoProducto = TipoProducto.Insumo, StockActual = 10 };
        _db.Productos.Add(insumo);
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.RegistrarEntradaAsync(
            _sucursalId, Guid.NewGuid(), new RegistrarEntradaRequestDto { ProductoId = insumo.Id, Cantidad = 0 }));
    }

    [Fact]
    public async Task RegistrarEntrada_SobreProductoVendible_Rechaza()
    {
        var vendible = new Producto { SucursalId = _sucursalId, Nombre = "Hamburguesa", TipoProducto = TipoProducto.Vendible };
        _db.Productos.Add(vendible);
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.RegistrarEntradaAsync(
            _sucursalId, Guid.NewGuid(), new RegistrarEntradaRequestDto { ProductoId = vendible.Id, Cantidad = 5 }));
    }

    [Fact]
    public async Task RegistrarAjuste_CorrigeElStockAlNuevoValorYRegistraElDelta()
    {
        var insumo = new Producto { SucursalId = _sucursalId, Nombre = "Queso", TipoProducto = TipoProducto.Insumo, StockActual = 20 };
        _db.Productos.Add(insumo);
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);
        await servicio.RegistrarAjusteAsync(_sucursalId, Guid.NewGuid(), new RegistrarAjusteRequestDto
        {
            ProductoId = insumo.Id,
            NuevoStock = 15, // conteo físico dio menos que el sistema (merma)
            Motivo = "Conteo físico mensual"
        });

        var actualizado = await _db.Productos.FindAsync(insumo.Id);
        Assert.Equal(15m, actualizado!.StockActual);

        var movimiento = Assert.Single(_db.MovimientosInventario);
        Assert.Equal(Dominio.Inventario.TipoMovimientoInventario.Ajuste, movimiento.Tipo);
        Assert.Equal(-5m, movimiento.Cantidad); // delta: 15 - 20
    }

    [Fact]
    public async Task RegistrarAjuste_MismoValor_Rechaza()
    {
        var insumo = new Producto { SucursalId = _sucursalId, Nombre = "Queso", TipoProducto = TipoProducto.Insumo, StockActual = 20 };
        _db.Productos.Add(insumo);
        await _db.SaveChangesAsync();

        var servicio = new InventarioAppService(_db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.RegistrarAjusteAsync(
            _sucursalId, Guid.NewGuid(),
            new RegistrarAjusteRequestDto { ProductoId = insumo.Id, NuevoStock = 20, Motivo = "Sin cambios" }));
    }
}
