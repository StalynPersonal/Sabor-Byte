using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Catalogo.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo;

public class ProductoAppService(IAppDbContext db)
{
    // Búsqueda rápida para Caja/Mesero: por código de barra exacto o por coincidencia parcial de descripción.
    public async Task<List<ProductoResumenDto>> BuscarAsync(Guid sucursalId, string texto, CancellationToken ct = default)
    {
        var query = db.Productos.Where(p =>
            p.SucursalId == sucursalId &&
            p.Activo &&
            p.TipoProducto == TipoProducto.Vendible);

        if (!string.IsNullOrWhiteSpace(texto))
        {
            query = query.Where(p =>
                p.CodigoBarra == texto ||
                EF.Functions.Like(p.Nombre, $"%{texto}%"));
        }

        return await query
            .OrderBy(p => p.Nombre)
            .Take(50)
            .Select(p => new ProductoResumenDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                ImagenUrl = p.ImagenUrl,
                CodigoBarra = p.CodigoBarra,
                Precio = p.Precio,
                AplicaItbis = p.AplicaItbis,
                TipoProducto = p.TipoProducto
            })
            .ToListAsync(ct);
    }

    // Listado completo para el módulo Central (todos los tipos, incluye inactivos si se pide).
    public async Task<List<ProductoDetalleDto>> ListarAsync(Guid sucursalId, bool incluirInactivos, CancellationToken ct = default)
    {
        var query = db.Productos.Where(p => p.SucursalId == sucursalId);
        if (!incluirInactivos)
            query = query.Where(p => p.Activo);

        return await query
            .OrderBy(p => p.Nombre)
            .Select(p => MapearDetalle(p))
            .ToListAsync(ct);
    }

    public async Task<ProductoDetalleDto> ObtenerAsync(Guid sucursalId, Guid productoId, CancellationToken ct = default)
    {
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == productoId && p.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El producto no existe.");

        return MapearDetalle(producto);
    }

    public async Task<Guid> CrearAsync(Guid sucursalId, GuardarProductoRequestDto request, CancellationToken ct = default)
    {
        var producto = new Producto
        {
            SucursalId = sucursalId,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            ImagenUrl = request.ImagenUrl,
            CodigoBarra = request.CodigoBarra,
            Precio = request.Precio,
            CostoUnitario = request.CostoUnitario,
            CategoriaId = request.CategoriaId,
            AplicaItbis = request.AplicaItbis,
            TipoProducto = request.TipoProducto,
            UnidadMedida = request.UnidadMedida,
            StockMinimo = request.StockMinimo,
            StockMaximo = request.StockMaximo
        };

        if (producto.TipoProducto == TipoProducto.Vendible)
        {
            foreach (var ingrediente in request.Receta)
            {
                producto.Receta.Add(new ProductoIngrediente
                {
                    ProductoId = producto.Id,
                    InsumoId = ingrediente.InsumoId,
                    CantidadUsada = ingrediente.CantidadUsada,
                    IncluidoPorDefecto = ingrediente.IncluidoPorDefecto,
                    Opcional = ingrediente.Opcional
                });
            }
        }

        db.Productos.Add(producto);
        await db.SaveChangesAsync(ct);

        return producto.Id;
    }

    public async Task ActualizarAsync(Guid sucursalId, Guid productoId, GuardarProductoRequestDto request, CancellationToken ct = default)
    {
        var producto = await db.Productos
            .Include(p => p.Receta)
            .FirstOrDefaultAsync(p => p.Id == productoId && p.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El producto no existe.");

        producto.Nombre = request.Nombre;
        producto.Descripcion = request.Descripcion;
        producto.ImagenUrl = request.ImagenUrl;
        producto.CodigoBarra = request.CodigoBarra;
        producto.Precio = request.Precio;
        producto.CostoUnitario = request.CostoUnitario;
        producto.CategoriaId = request.CategoriaId;
        producto.AplicaItbis = request.AplicaItbis;
        producto.UnidadMedida = request.UnidadMedida;
        producto.StockMinimo = request.StockMinimo;
        producto.StockMaximo = request.StockMaximo;
        // TipoProducto no se permite cambiar tras crearlo (evita inconsistencias con
        // movimientos de inventario / recetas ya existentes).

        if (producto.TipoProducto == TipoProducto.Vendible)
        {
            db.ProductoIngredientes.RemoveRange(producto.Receta);
            producto.Receta.Clear();
            foreach (var ingrediente in request.Receta)
            {
                producto.Receta.Add(new ProductoIngrediente
                {
                    ProductoId = producto.Id,
                    InsumoId = ingrediente.InsumoId,
                    CantidadUsada = ingrediente.CantidadUsada,
                    IncluidoPorDefecto = ingrediente.IncluidoPorDefecto,
                    Opcional = ingrediente.Opcional
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task DesactivarAsync(Guid sucursalId, Guid productoId, CancellationToken ct = default)
    {
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == productoId && p.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El producto no existe.");

        producto.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    // Crea un producto Vendible marcado como combo, con sus componentes (otros
    // productos Vendibles). El descuento de inventario se resuelve expandiendo cada
    // componente a su propia receta (ver InventarioAppService.ObtenerRecetaEfectivaAsync).
    public async Task<Guid> CrearComboAsync(Guid sucursalId, CrearComboRequestDto request, CancellationToken ct = default)
    {
        if (request.Componentes.Count == 0)
            throw new InvalidOperationException("El combo debe tener al menos un componente.");

        var componenteIds = request.Componentes.Select(c => c.ProductoIncluidoId).ToList();
        var existentes = await db.Productos
            .Where(p => componenteIds.Contains(p.Id) && p.TipoProducto == TipoProducto.Vendible)
            .Select(p => p.Id)
            .ToListAsync(ct);

        var faltante = componenteIds.Except(existentes).FirstOrDefault();
        if (faltante != Guid.Empty)
            throw new InvalidOperationException($"El producto {faltante} no existe o no es vendible.");

        var combo = new Producto
        {
            SucursalId = sucursalId,
            Nombre = request.Nombre,
            Precio = request.Precio,
            CategoriaId = request.CategoriaId,
            TipoProducto = TipoProducto.Vendible,
            EsCombo = true
        };

        foreach (var componente in request.Componentes)
        {
            combo.ComponentesCombo.Add(new ComboItem
            {
                ComboId = combo.Id,
                ProductoIncluidoId = componente.ProductoIncluidoId,
                Cantidad = componente.Cantidad
            });
        }

        db.Productos.Add(combo);
        await db.SaveChangesAsync(ct);

        return combo.Id;
    }

    private static ProductoDetalleDto MapearDetalle(Producto p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        ImagenUrl = p.ImagenUrl,
        CodigoBarra = p.CodigoBarra,
        Precio = p.Precio,
        CostoUnitario = p.CostoUnitario,
        CategoriaId = p.CategoriaId,
        Activo = p.Activo,
        AplicaItbis = p.AplicaItbis,
        TipoProducto = p.TipoProducto,
        UnidadMedida = p.UnidadMedida,
        StockMinimo = p.StockMinimo,
        StockMaximo = p.StockMaximo,
        StockActual = p.StockActual,
        EsCombo = p.EsCombo
    };
}
