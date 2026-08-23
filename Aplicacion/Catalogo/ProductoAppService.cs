using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Catalogo.Dtos;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo;

// Catálogo de TODA la empresa (una Empresa, varias Sucursales) — Producto y Categoria ya
// no son por sucursal (ver comentario de clase en Producto.cs). Lo único que sigue siendo
// por sucursal es el STOCK (ver StockSucursal / InventarioAppService), así que estos
// métodos ya no reciben sucursalId salvo donde hace falta para mostrar/validar stock.
public class ProductoAppService(IAppDbContext db)
{
    // Búsqueda rápida para Caja/Mesero: por código de barra exacto (cualquiera de los
    // asociados al producto) o por coincidencia parcial de nombre/código propio.
    public async Task<List<ProductoResumenDto>> BuscarAsync(string texto, Guid? categoriaId = null, CancellationToken ct = default)
    {
        var query = db.Productos.Where(p => p.Activo && p.TipoProducto == TipoProducto.Vendible);

        if (categoriaId is not null)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(texto))
        {
            query = query.Where(p =>
                p.CodigosBarra.Any(cb => cb.CodigoBarra == texto) ||
                p.Codigo == texto ||
                EF.Functions.Like(p.Nombre, $"%{texto}%"));
        }

        return await (
                from p in query
                join c in db.Categorias on p.CategoriaId equals c.Id
                orderby p.Nombre
                select new ProductoResumenDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Codigo = p.Codigo,
                    ImagenUrl = p.ImagenUrl,
                    Precio = p.Precio,
                    TasaItbis = p.TasaItbis,
                    TipoProducto = p.TipoProducto,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = c.Nombre
                })
            .Take(50)
            .ToListAsync(ct);
    }

    // Listado paginado para el módulo Central, con filtros de búsqueda. sucursalId es
    // opcional y solo se usa para traer el stock de esa sucursal en cada fila de Insumo.
    public async Task<ResultadoPaginado<ProductoDetalleDto>> ListarAsync(
        int pagina, int tamanoPagina, string? texto, TipoProducto? tipo, bool incluirInactivos,
        Guid? sucursalId, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 200);

        var query = db.Productos.AsQueryable();

        if (!incluirInactivos)
            query = query.Where(p => p.Activo);

        if (tipo is not null)
            query = query.Where(p => p.TipoProducto == tipo);

        if (!string.IsNullOrWhiteSpace(texto))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.Nombre, $"%{texto}%") ||
                (p.Codigo != null && EF.Functions.Like(p.Codigo, $"%{texto}%")) ||
                p.CodigosBarra.Any(cb => EF.Functions.Like(cb.CodigoBarra, $"%{texto}%")));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Nombre)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(p => MapearDetalle(p))
            .ToListAsync(ct);

        if (sucursalId is Guid sid && items.Count > 0)
            await AdjuntarStockAsync(items, sid, ct);

        return new ResultadoPaginado<ProductoDetalleDto>
        {
            Items = items,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = total
        };
    }

    // Todo lo que lleva stock propio en una sucursal: Insumos y Vendibles-Inventariables
    // (ej. agua embotellada) — usado por la pantalla "Inventario" de Central, que ya no
    // distingue por TipoProducto sino por esta bandera.
    public async Task<List<ProductoDetalleDto>> ListarInventariablesAsync(Guid sucursalId, string? texto, CancellationToken ct = default)
    {
        var query = db.Productos.Where(p => p.Activo && p.Inventariable);

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(p => EF.Functions.Like(p.Nombre, $"%{texto}%"));

        var items = await query
            .OrderBy(p => p.Nombre)
            .Select(p => MapearDetalle(p))
            .ToListAsync(ct);

        if (items.Count > 0)
            await AdjuntarStockAsync(items, sucursalId, ct);

        return items;
    }

    // sucursalId es opcional: si se pasa, el detalle trae el stock de esa sucursal
    // (solo aplica cuando el producto es Insumo).
    public async Task<ProductoDetalleDto> ObtenerAsync(Guid productoId, Guid? sucursalId, CancellationToken ct = default)
    {
        var producto = await db.Productos
            .Include(p => p.CodigosBarra)
            .Include(p => p.Receta).ThenInclude(r => r.Insumo)
            .FirstOrDefaultAsync(p => p.Id == productoId, ct)
            ?? throw new InvalidOperationException("El producto no existe.");

        var dto = MapearDetalle(producto);
        dto.Receta = producto.Receta.Select(r => new RecetaItemDto
        {
            InsumoId = r.InsumoId,
            InsumoNombre = r.Insumo?.Nombre ?? "?",
            CantidadUsada = r.CantidadUsada,
            IncluidoPorDefecto = r.IncluidoPorDefecto,
            Opcional = r.Opcional
        }).ToList();

        if (sucursalId is Guid sid)
            await AdjuntarStockAsync([dto], sid, ct);

        var usuarioIds = new List<Guid> { producto.CreadoPorUsuarioId };
        if (producto.ActualizadoPorUsuarioId is Guid idActualizador)
            usuarioIds.Add(idActualizador);

        var nombresUsuarios = await db.Usuarios
            .Where(u => usuarioIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Nombre, ct);

        dto.CreadoPorNombre = nombresUsuarios.GetValueOrDefault(producto.CreadoPorUsuarioId);
        dto.ActualizadoPorNombre = producto.ActualizadoPorUsuarioId is Guid idActualizador2
            ? nombresUsuarios.GetValueOrDefault(idActualizador2)
            : null;

        return dto;
    }

    public async Task<Guid> CrearAsync(Guid usuarioId, GuardarProductoRequestDto request, CancellationToken ct = default)
    {
        ValidarNombre(request.Nombre);
        ValidarPrecioYCosto(request.Precio, request.CostoUnitario);
        await ValidarCodigosAsync(request.Codigo, request.CodigosBarra, productoId: null, ct);
        await ValidarCategoriaAsync(request.CategoriaId, ct);
        await ValidarUnidadMedidaAsync(request.UnidadMedidaId, ct);
        ValidarReceta(request.Receta);

        // Todo Insumo es inventariable por definición; para un Vendible lo decide el
        // formulario (ver comentario en Producto.Inventariable).
        var inventariable = request.TipoProducto == TipoProducto.Insumo || request.Inventariable;
        ValidarInventariableVsReceta(request.TipoProducto, inventariable, request.Receta);

        var producto = new Producto
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            ImagenUrl = request.ImagenUrl,
            Codigo = request.Codigo,
            Precio = request.Precio,
            CostoUnitario = request.CostoUnitario ?? 0m,
            CategoriaId = request.CategoriaId,
            TasaItbis = request.TasaItbis,
            TipoProducto = request.TipoProducto,
            Inventariable = inventariable,
            UnidadMedidaId = request.UnidadMedidaId,
            CreadoPorUsuarioId = usuarioId
        };

        AsignarCodigosBarra(producto, request.CodigosBarra);

        if (producto.TipoProducto == TipoProducto.Vendible && !inventariable)
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

        // Producto inventariable nuevo (Insumo, o Vendible de reventa): arranca en 0 en
        // TODAS las sucursales existentes — cada una configura su propio mínimo/máximo/
        // entrada inicial después, desde Inventario.
        if (inventariable)
        {
            var sucursalIds = await db.Sucursales.Select(s => s.Id).ToListAsync(ct);
            db.StockPorSucursal.AddRange(sucursalIds.Select(sid => new StockSucursal
            {
                ProductoId = producto.Id,
                SucursalId = sid
            }));
        }

        await db.SaveChangesAsync(ct);

        return producto.Id;
    }

    public async Task ActualizarAsync(Guid productoId, Guid usuarioId, GuardarProductoRequestDto request, CancellationToken ct = default)
    {
        var producto = await db.Productos
            .Include(p => p.Receta)
            .Include(p => p.CodigosBarra)
            .FirstOrDefaultAsync(p => p.Id == productoId, ct)
            ?? throw new InvalidOperationException("El producto no existe.");

        ValidarNombre(request.Nombre);
        ValidarPrecioYCosto(request.Precio, request.CostoUnitario);
        await ValidarCodigosAsync(request.Codigo, request.CodigosBarra, productoId, ct);
        await ValidarCategoriaAsync(request.CategoriaId, ct);
        await ValidarUnidadMedidaAsync(request.UnidadMedidaId, ct);
        ValidarReceta(request.Receta);

        var inventariable = producto.TipoProducto == TipoProducto.Insumo || request.Inventariable;
        ValidarInventariableVsReceta(producto.TipoProducto, inventariable, request.Receta);

        var pasaAInventariable = inventariable && !producto.Inventariable;

        producto.Nombre = request.Nombre;
        producto.Descripcion = request.Descripcion;
        producto.ImagenUrl = request.ImagenUrl;
        producto.Codigo = request.Codigo;
        producto.Precio = request.Precio;
        producto.CostoUnitario = request.CostoUnitario ?? 0m;
        producto.CategoriaId = request.CategoriaId;
        producto.TasaItbis = request.TasaItbis;
        producto.Inventariable = inventariable;
        producto.UnidadMedidaId = request.UnidadMedidaId;
        producto.ActualizadoEn = DateTime.UtcNow;
        producto.ActualizadoPorUsuarioId = usuarioId;
        // TipoProducto no se permite cambiar tras crearlo (evita inconsistencias con
        // movimientos de inventario / recetas ya existentes).

        // Si un Vendible pasa de no-inventariable a inventariable después de creado, hay
        // que crearle su renglón de stock en cada sucursal (mismo caso que un Insumo nuevo
        // en CrearAsync) — si no, la primera venta lo encontraría sin StockSucursal.
        if (pasaAInventariable)
        {
            var sucursalIds = await db.Sucursales.Select(s => s.Id).ToListAsync(ct);
            var existentes = await db.StockPorSucursal
                .Where(s => s.ProductoId == producto.Id)
                .Select(s => s.SucursalId)
                .ToListAsync(ct);
            db.StockPorSucursal.AddRange(sucursalIds.Except(existentes).Select(sid => new StockSucursal
            {
                ProductoId = producto.Id,
                SucursalId = sid
            }));
        }

        // Reemplazo completo de códigos de barra: se agregan directo al DbSet (no a
        // producto.CodigosBarra) porque RemoveRange + Clear + Add sobre la misma colección
        // de navegación en la misma unidad de trabajo confunde al change tracker — llegó a
        // generar un UPDATE para la fila nueva en vez de un INSERT, y esa fila nunca existía
        // (DbUpdateConcurrencyException: "expected to affect 1 row(s), but actually affected 0").
        db.CodigosBarraProducto.RemoveRange(producto.CodigosBarra);
        producto.CodigosBarra.Clear();
        db.CodigosBarraProducto.AddRange(request.CodigosBarra
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .Select(codigo => new CodigoBarraProducto { ProductoId = producto.Id, CodigoBarra = codigo.Trim() }));

        if (producto.TipoProducto == TipoProducto.Vendible)
        {
            db.ProductoIngredientes.RemoveRange(producto.Receta);
            producto.Receta.Clear();

            // Inventariable y receta son mutuamente excluyentes (ver ValidarInventariableVsReceta):
            // si es inventariable, request.Receta ya llega vacío; no hace falta un "if" aparte.
            foreach (var ingrediente in request.Receta)
            {
                db.ProductoIngredientes.Add(new ProductoIngrediente
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

    public async Task DesactivarAsync(Guid productoId, CancellationToken ct = default)
    {
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == productoId, ct)
            ?? throw new InvalidOperationException("El producto no existe.");

        producto.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task ActivarAsync(Guid productoId, CancellationToken ct = default)
    {
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == productoId, ct)
            ?? throw new InvalidOperationException("El producto no existe.");

        producto.Activo = true;
        await db.SaveChangesAsync(ct);
    }

    // Crea un producto Vendible marcado como combo, con sus componentes (otros
    // productos Vendibles). El descuento de inventario se resuelve expandiendo cada
    // componente a su propia receta (ver InventarioAppService.ObtenerRecetaEfectivaAsync).
    public async Task<Guid> CrearComboAsync(Guid usuarioId, CrearComboRequestDto request, CancellationToken ct = default)
    {
        ValidarNombre(request.Nombre);
        ValidarPrecioYCosto(request.Precio, null);

        if (request.Componentes.Count == 0)
            throw new InvalidOperationException("El combo debe tener al menos un componente.");

        if (request.Componentes.Any(c => c.Cantidad < 1))
            throw new InvalidOperationException("La cantidad de cada componente del combo debe ser como mínimo 1.");

        var componenteIds = request.Componentes.Select(c => c.ProductoIncluidoId).ToList();
        var existentes = await db.Productos
            .Where(p => componenteIds.Contains(p.Id) && p.TipoProducto == TipoProducto.Vendible)
            .Select(p => p.Id)
            .ToListAsync(ct);

        var faltante = componenteIds.Except(existentes).FirstOrDefault();
        if (faltante != Guid.Empty)
            throw new InvalidOperationException($"El producto {faltante} no existe o no es vendible.");

        await ValidarCodigosAsync(request.Codigo, [], productoId: null, ct);
        await ValidarCategoriaAsync(request.CategoriaId, ct);

        // Un combo se vende como una unidad completa, no tiene sentido elegir su propia
        // unidad de medida — se asigna la unidad "Unidad" del catálogo automáticamente.
        var unidadPorDefecto = await db.UnidadesMedida.FirstOrDefaultAsync(u => u.Nombre == "Unidad", ct)
            ?? await db.UnidadesMedida.FirstOrDefaultAsync(u => u.Activo, ct)
            ?? throw new InvalidOperationException("No hay ninguna unidad de medida configurada en el catálogo.");

        var combo = new Producto
        {
            Nombre = request.Nombre,
            Codigo = request.Codigo,
            Precio = request.Precio,
            CategoriaId = request.CategoriaId,
            TipoProducto = TipoProducto.Vendible,
            UnidadMedidaId = unidadPorDefecto.Id,
            EsCombo = true,
            CreadoPorUsuarioId = usuarioId
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

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
    }

    private static void ValidarPrecioYCosto(decimal precio, decimal? costoUnitario)
    {
        if (precio < 0)
            throw new InvalidOperationException("El precio no puede ser negativo.");
        if (costoUnitario < 0)
            throw new InvalidOperationException("El costo unitario no puede ser negativo.");
    }

    // Código propio y códigos de barra comparten el mismo "espacio de nombres" para
    // búsqueda (ver BuscarAsync/ListarAsync, que buscan en ambos indistintamente) — así
    // que ninguno de los códigos de este producto (el propio ni sus barras) puede coincidir
    // con el código propio NI con un código de barra de NINGÚN OTRO producto del catálogo
    // (que ahora es único para toda la empresa, no por sucursal).
    private async Task ValidarCodigosAsync(string codigoPrincipal, List<string> codigosBarra, Guid? productoId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(codigoPrincipal))
            throw new InvalidOperationException("El código del producto es obligatorio.");

        var codigosBarraLimpios = codigosBarra
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct()
            .ToList();

        if (codigosBarraLimpios.Contains(codigoPrincipal))
            throw new InvalidOperationException($"El código '{codigoPrincipal}' no puede repetirse como código de barra del mismo producto.");

        var todosLosCodigos = new List<string> { codigoPrincipal };
        todosLosCodigos.AddRange(codigosBarraLimpios);

        var conflictos = await db.Productos
            .Where(p => p.Id != productoId &&
                (todosLosCodigos.Contains(p.Codigo!) || p.CodigosBarra.Any(cb => todosLosCodigos.Contains(cb.CodigoBarra))))
            .Select(p => new { p.Codigo, CodigosBarra = p.CodigosBarra.Select(cb => cb.CodigoBarra).ToList() })
            .ToListAsync(ct);

        foreach (var otro in conflictos)
        {
            if (otro.Codigo is not null && todosLosCodigos.Contains(otro.Codigo))
                throw new InvalidOperationException($"El código '{otro.Codigo}' ya está en uso por otro producto del catálogo.");

            var repetido = otro.CodigosBarra.FirstOrDefault(todosLosCodigos.Contains);
            if (repetido is not null)
                throw new InvalidOperationException($"El código '{repetido}' ya está registrado como código de barra de otro producto del catálogo.");
        }
    }

    private static void ValidarReceta(List<IngredienteRequestDto> receta)
    {
        if (receta.Any(i => i.CantidadUsada <= 0))
            throw new InvalidOperationException("La cantidad usada de cada ingrediente de la receta debe ser mayor a cero.");
    }

    // Un producto Inventariable (Insumo, o Vendible de reventa como una botella de agua)
    // lleva su propio stock — no tiene sentido que ADEMÁS tenga una receta que descuente
    // otros insumos: sería ambiguo cuál de las dos formas de descuento aplica al vender.
    private static void ValidarInventariableVsReceta(TipoProducto tipo, bool inventariable, List<IngredienteRequestDto> receta)
    {
        if (tipo == TipoProducto.Vendible && inventariable && receta.Count > 0)
            throw new InvalidOperationException(
                "Un producto Vendible marcado como Inventariable no puede tener receta — son mutuamente excluyentes: o se prepara (receta) o se revende tal cual (stock propio).");
    }

    private async Task ValidarCategoriaAsync(Guid categoriaId, CancellationToken ct)
    {
        var existe = await db.Categorias.AnyAsync(c => c.Id == categoriaId, ct);
        if (!existe)
            throw new InvalidOperationException("La categoría seleccionada no existe.");
    }

    private async Task ValidarUnidadMedidaAsync(Guid unidadMedidaId, CancellationToken ct)
    {
        var existe = await db.UnidadesMedida.AnyAsync(u => u.Id == unidadMedidaId, ct);
        if (!existe)
            throw new InvalidOperationException("La unidad de medida seleccionada no existe.");
    }

    private static void AsignarCodigosBarra(Producto producto, List<string> codigosBarra)
    {
        foreach (var codigo in codigosBarra.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct())
        {
            producto.CodigosBarra.Add(new CodigoBarraProducto
            {
                ProductoId = producto.Id,
                CodigoBarra = codigo.Trim()
            });
        }
    }

    // Completa StockActual/Mínimo/Máximo de cada dto de tipo Insumo con lo que haya en
    // StockSucursal para la sucursal indicada (0/null si todavía no existe el renglón).
    private async Task AdjuntarStockAsync(List<ProductoDetalleDto> productos, Guid sucursalId, CancellationToken ct)
    {
        var inventariableIds = productos.Where(p => p.Inventariable).Select(p => p.Id).ToList();
        if (inventariableIds.Count == 0)
            return;

        var stocks = await db.StockPorSucursal
            .Where(s => s.SucursalId == sucursalId && inventariableIds.Contains(s.ProductoId))
            .ToDictionaryAsync(s => s.ProductoId, s => s, ct);

        foreach (var dto in productos)
        {
            if (!dto.Inventariable)
                continue;

            if (stocks.TryGetValue(dto.Id, out var stock))
            {
                dto.StockActual = stock.StockActual;
                dto.StockMinimo = stock.StockMinimo;
                dto.StockMaximo = stock.StockMaximo;
            }
        }
    }

    private static ProductoDetalleDto MapearDetalle(Producto p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        ImagenUrl = p.ImagenUrl,
        Codigo = p.Codigo,
        CodigosBarra = p.CodigosBarra.Select(cb => cb.CodigoBarra).ToList(),
        Precio = p.Precio,
        CostoUnitario = p.CostoUnitario,
        CategoriaId = p.CategoriaId,
        Activo = p.Activo,
        TasaItbis = p.TasaItbis,
        TipoProducto = p.TipoProducto,
        Inventariable = p.Inventariable,
        UnidadMedidaId = p.UnidadMedidaId,
        EsCombo = p.EsCombo,
        CreadoEn = p.CreadoEn,
        ActualizadoEn = p.ActualizadoEn
    };
}
