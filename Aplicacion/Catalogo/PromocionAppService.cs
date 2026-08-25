using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Catalogo.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo;

public class PromocionAppService(IAppDbContext db)
{
    public async Task<List<PromocionDto>> ListarAsync(CancellationToken ct = default)
    {
        var promociones = await db.Promociones
            .OrderByDescending(p => p.FechaInicio)
            .ToListAsync(ct);

        return await MapearAsync(promociones, ct);
    }

    // Usado por Caja (preview en el carrito) y por VentaAppService (cálculo autoritativo
    // al facturar): promociones activas, vigentes hoy, aplicables a esta sucursal (propias
    // de la sucursal o sin sucursal asignada = todas).
    public async Task<List<PromocionDto>> ListarVigentesAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;
        var promociones = await db.Promociones
            .Where(p => p.Activo
                && p.FechaInicio <= ahora && ahora <= p.FechaFin
                && (p.SucursalId == null || p.SucursalId == sucursalId))
            .ToListAsync(ct);

        return await MapearAsync(promociones, ct);
    }

    public async Task<Guid> CrearAsync(GuardarPromocionRequestDto request, CancellationToken ct = default)
    {
        ValidarDatos(request);

        var promocion = new Promocion
        {
            SucursalId = request.SucursalId,
            Nombre = request.Nombre,
            ProductoId = request.ProductoId,
            CategoriaId = request.CategoriaId,
            TipoDescuento = request.TipoDescuento,
            Valor = request.Valor,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Activo = request.Activo
        };

        promocion.Codigo = await GenerarCodigoAsync(ct);

        db.Promociones.Add(promocion);
        await db.SaveChangesAsync(ct);
        return promocion.Id;
    }

    // Solo el Admin crea promociones, muy esporádicamente (a diferencia de facturas/NCF,
    // que se generan por cada venta) — un simple MAX+1 basta, no hace falta el patrón de
    // reserva con compare-and-swap que usa VentaAppService para números de alta concurrencia.
    private async Task<string> GenerarCodigoAsync(CancellationToken ct)
    {
        var ultimoNumero = await db.Promociones
            .Where(p => p.Codigo.StartsWith("PROMO-"))
            .Select(p => p.Codigo)
            .ToListAsync(ct);

        var maximo = ultimoNumero
            .Select(c => int.TryParse(c.AsSpan(6), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"PROMO-{(maximo + 1):D5}";
    }

    public async Task ActualizarAsync(Guid promocionId, GuardarPromocionRequestDto request, CancellationToken ct = default)
    {
        var promocion = await db.Promociones.FirstOrDefaultAsync(p => p.Id == promocionId, ct)
            ?? throw new InvalidOperationException("La promoción no existe.");

        ValidarDatos(request);

        promocion.SucursalId = request.SucursalId;
        promocion.Nombre = request.Nombre;
        promocion.ProductoId = request.ProductoId;
        promocion.CategoriaId = request.CategoriaId;
        promocion.TipoDescuento = request.TipoDescuento;
        promocion.Valor = request.Valor;
        promocion.FechaInicio = request.FechaInicio;
        promocion.FechaFin = request.FechaFin;
        promocion.Activo = request.Activo;

        await db.SaveChangesAsync(ct);
    }

    private static void ValidarDatos(GuardarPromocionRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre de la promoción es obligatorio.");

        var tieneProducto = request.ProductoId is not null;
        var tieneCategoria = request.CategoriaId is not null;
        if (tieneProducto == tieneCategoria)
            throw new InvalidOperationException("La promoción debe aplicar a un producto O a una categoría (no ambos, no ninguno).");

        if (request.Valor <= 0)
            throw new InvalidOperationException("El valor del descuento debe ser mayor a cero.");

        if (request.TipoDescuento == TipoDescuentoPromocion.Porcentaje && request.Valor > 100)
            throw new InvalidOperationException("Un descuento porcentual no puede ser mayor a 100%.");

        if (request.FechaFin < request.FechaInicio)
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la fecha de inicio.");
    }

    private async Task<List<PromocionDto>> MapearAsync(List<Promocion> promociones, CancellationToken ct)
    {
        var productoIds = promociones.Where(p => p.ProductoId is not null).Select(p => p.ProductoId!.Value).Distinct().ToList();
        var categoriaIds = promociones.Where(p => p.CategoriaId is not null).Select(p => p.CategoriaId!.Value).Distinct().ToList();
        var sucursalIds = promociones.Where(p => p.SucursalId is not null).Select(p => p.SucursalId!.Value).Distinct().ToList();

        var nombresProductos = await db.Productos.Where(p => productoIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Nombre, ct);
        var nombresCategorias = await db.Categorias.Where(c => categoriaIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Nombre, ct);
        var nombresSucursales = await db.Sucursales.Where(s => sucursalIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Nombre, ct);

        return promociones.Select(p => new PromocionDto
        {
            Id = p.Id,
            Codigo = p.Codigo,
            SucursalId = p.SucursalId,
            SucursalNombre = p.SucursalId is Guid sId ? nombresSucursales.GetValueOrDefault(sId, "?") : "Todas",
            Nombre = p.Nombre,
            ProductoId = p.ProductoId,
            ProductoNombre = p.ProductoId is Guid pId ? nombresProductos.GetValueOrDefault(pId, "?") : null,
            CategoriaId = p.CategoriaId,
            CategoriaNombre = p.CategoriaId is Guid cId ? nombresCategorias.GetValueOrDefault(cId, "?") : null,
            TipoDescuento = p.TipoDescuento,
            Valor = p.Valor,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Activo = p.Activo
        }).ToList();
    }
}
