using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.Cotizaciones.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Cotizaciones;

namespace SaborByte.Aplicacion.Cotizaciones;

// Presupuestos para pedidos/eventos futuros — ver comentario de clase en Cotizacion.
// Se crean desde Central o desde Caja indistintamente; cuando el cliente confirma se
// marca Aceptada y cualquier cajero la carga en el carrito para facturarla con los mismos
// precios con que se cotizó.
public class CotizacionAppService(IAppDbContext db)
{
    public async Task<ResultadoPaginado<CotizacionResumenDto>> ListarAsync(
        Guid sucursalId, string? texto, EstadoCotizacion? estado, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        var consulta =
            from c in db.Cotizaciones
            join u in db.Usuarios on c.CreadoPorUsuarioId equals u.Id
            where c.SucursalId == sucursalId
            select new { Cotizacion = c, CreadoPorNombre = u.Nombre };

        if (!string.IsNullOrWhiteSpace(texto))
            consulta = consulta.Where(x => x.Cotizacion.ClienteNombre.Contains(texto));

        if (estado is not null)
            consulta = consulta.Where(x => x.Cotizacion.Estado == estado);

        var totalRegistros = await consulta.CountAsync(ct);

        var items = await consulta
            .OrderByDescending(x => x.Cotizacion.CreadoEn)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(x => new CotizacionResumenDto
            {
                Id = x.Cotizacion.Id,
                ClienteNombre = x.Cotizacion.ClienteNombre,
                FechaVencimiento = x.Cotizacion.FechaVencimiento,
                Estado = x.Cotizacion.Estado,
                CantidadItems = x.Cotizacion.Items.Count,
                Total = x.Cotizacion.Items.Sum(i => i.Precio * i.Cantidad * (1 + i.TasaItbis)),
                CreadoEn = x.Cotizacion.CreadoEn,
                CreadoPorNombre = x.CreadoPorNombre,
                YaCargadaEnCarrito = x.Cotizacion.CargadaEnCarritoEn != null
            })
            .ToListAsync(ct);

        return new ResultadoPaginado<CotizacionResumenDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = totalRegistros };
    }

    public async Task<CotizacionDetalleDto> ObtenerAsync(Guid sucursalId, Guid id, CancellationToken ct = default)
    {
        var cotizacion = await db.Cotizaciones
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cotización no existe.");

        var creadoPorNombre = await db.Usuarios.Where(u => u.Id == cotizacion.CreadoPorUsuarioId).Select(u => u.Nombre).FirstOrDefaultAsync(ct);

        return AMapaDetalle(cotizacion, creadoPorNombre);
    }

    private static CotizacionDetalleDto AMapaDetalle(Cotizacion cotizacion, string? creadoPorNombre) => new()
    {
        Id = cotizacion.Id,
        SucursalId = cotizacion.SucursalId,
        ClienteId = cotizacion.ClienteId,
        ClienteNombre = cotizacion.ClienteNombre,
        ClienteTelefono = cotizacion.ClienteTelefono,
        FechaVencimiento = cotizacion.FechaVencimiento,
        Notas = cotizacion.Notas,
        Estado = cotizacion.Estado,
        CreadoEn = cotizacion.CreadoEn,
        CreadoPorNombre = creadoPorNombre,
        YaCargadaEnCarrito = cotizacion.CargadaEnCarritoEn != null,
        Items = cotizacion.Items.Select(i => new CotizacionItemDto
        {
            ProductoId = i.ProductoId,
            CategoriaId = i.CategoriaId,
            NombreProducto = i.NombreProducto,
            Precio = i.Precio,
            TasaItbis = i.TasaItbis,
            Cantidad = i.Cantidad
        }).ToList()
    };

    public async Task<Guid> CrearAsync(Guid sucursalId, Guid usuarioId, GuardarCotizacionRequestDto request, CancellationToken ct = default)
    {
        var cotizacion = await ConstruirDesdeRequestAsync(sucursalId, usuarioId, request, DateTime.UtcNow, ct);
        db.Cotizaciones.Add(cotizacion);
        await db.SaveChangesAsync(ct);
        return cotizacion.Id;
    }

    public async Task ActualizarAsync(Guid sucursalId, Guid id, Guid usuarioId, GuardarCotizacionRequestDto request, CancellationToken ct = default)
    {
        var existente = await db.Cotizaciones
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cotización no existe.");

        var reconstruida = await ConstruirDesdeRequestAsync(sucursalId, usuarioId, request, existente.CreadoEn, ct);

        existente.ClienteId = reconstruida.ClienteId;
        existente.ClienteNombre = reconstruida.ClienteNombre;
        existente.ClienteTelefono = reconstruida.ClienteTelefono;
        existente.FechaVencimiento = reconstruida.FechaVencimiento;
        existente.Notas = reconstruida.Notas;

        existente.Items.Clear();
        foreach (var item in reconstruida.Items)
            existente.Items.Add(item);

        await db.SaveChangesAsync(ct);
    }

    private async Task<Cotizacion> ConstruirDesdeRequestAsync(
        Guid sucursalId, Guid usuarioId, GuardarCotizacionRequestDto request, DateTime creadoEn, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ClienteNombre))
            throw new InvalidOperationException("El nombre del cliente es obligatorio.");

        if (request.Items.Count == 0)
            throw new InvalidOperationException("La cotización debe tener al menos un producto.");

        var productoIds = request.Items.Select(i => i.ProductoId).ToList();
        var productos = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var cotizacion = new Cotizacion
        {
            SucursalId = sucursalId,
            ClienteId = request.ClienteId,
            ClienteNombre = request.ClienteNombre,
            ClienteTelefono = request.ClienteTelefono,
            FechaVencimiento = request.FechaVencimiento,
            Notas = request.Notas,
            CreadoPorUsuarioId = usuarioId,
            CreadoEn = creadoEn
        };

        foreach (var item in request.Items)
        {
            if (!productos.TryGetValue(item.ProductoId, out var producto))
                throw new InvalidOperationException($"El producto {item.ProductoId} no existe.");

            cotizacion.Items.Add(new CotizacionItem
            {
                ProductoId = producto.Id,
                CategoriaId = producto.CategoriaId,
                NombreProducto = producto.Nombre,
                Precio = producto.Precio,
                TasaItbis = producto.TasaItbis,
                Cantidad = item.Cantidad
            });
        }

        return cotizacion;
    }

    public async Task CambiarEstadoAsync(Guid sucursalId, Guid id, EstadoCotizacion nuevoEstado, CancellationToken ct = default)
    {
        var cotizacion = await db.Cotizaciones.FirstOrDefaultAsync(c => c.Id == id && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cotización no existe.");

        cotizacion.Estado = nuevoEstado;
        await db.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(Guid sucursalId, Guid id, CancellationToken ct = default)
    {
        var cotizacion = await db.Cotizaciones.FirstOrDefaultAsync(c => c.Id == id && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cotización no existe.");

        db.Cotizaciones.Remove(cotizacion);
        await db.SaveChangesAsync(ct);
    }

    // Cargar en el carrito de Caja: entrega los precios tal como se cotizaron (no el precio
    // actual del producto) — es lo que se le prometió al cliente. Solo se puede cargar una
    // cotización Aceptada. No bloquea cargarla más de una vez (por si algo se canceló antes
    // de facturar la primera vez), solo deja constancia de que ya se usó al menos una vez.
    public async Task<CargaCarritoCotizacionDto> CargarEnCarritoAsync(Guid sucursalId, Guid id, CancellationToken ct = default)
    {
        var cotizacion = await db.Cotizaciones
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cotización no existe.");

        if (cotizacion.Estado != EstadoCotizacion.Aceptada)
            throw new InvalidOperationException("Solo se puede cargar en el carrito una cotización Aceptada.");

        cotizacion.CargadaEnCarritoEn ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new CargaCarritoCotizacionDto
        {
            ClienteId = cotizacion.ClienteId,
            ClienteNombre = cotizacion.ClienteNombre,
            Items = cotizacion.Items.Select(i => new CotizacionItemDto
            {
                ProductoId = i.ProductoId,
                NombreProducto = i.NombreProducto,
                Precio = i.Precio,
                TasaItbis = i.TasaItbis,
                Cantidad = i.Cantidad
            }).ToList()
        };
    }

    // "Recrear": arma una cotización nueva a partir de una existente, con los precios
    // ACTUALES del producto (no los congelados de la vieja) y fecha de hoy — para cuando
    // pasó tiempo desde la cotización original y los precios ya cambiaron. La original
    // queda intacta, sin tocarse.
    public async Task<Guid> RecrearAsync(Guid sucursalId, Guid id, Guid usuarioId, CancellationToken ct = default)
    {
        var original = await db.Cotizaciones
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cotización no existe.");

        var request = new GuardarCotizacionRequestDto
        {
            ClienteId = original.ClienteId,
            ClienteNombre = original.ClienteNombre,
            ClienteTelefono = original.ClienteTelefono,
            FechaVencimiento = DateTime.Today.AddDays(15),
            Notas = original.Notas,
            Items = original.Items.Select(i => new ItemCotizacionRequestDto { ProductoId = i.ProductoId, Cantidad = i.Cantidad }).ToList()
        };

        return await CrearAsync(sucursalId, usuarioId, request, ct);
    }
}
