using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Ventas.Dtos;
using SaborByte.Dominio.Ventas;

namespace SaborByte.Aplicacion.Ventas;

// Carritos de Caja puestos en pausa (ver comentario de clase en VentaSuspendida) — lista
// corta y de corta vida, como las comandas abiertas, así que no se pagina (igual criterio
// que ComandaAppService.ObtenerAbiertasAsync).
public class VentaSuspendidaAppService(IAppDbContext db)
{
    public async Task<List<VentaSuspendidaResumenDto>> ListarAsync(Guid sucursalId, CancellationToken ct = default) =>
        await (
                from v in db.VentasSuspendidas
                join u in db.Usuarios on v.CreadoPorUsuarioId equals u.Id
                where v.SucursalId == sucursalId
                orderby v.CreadoEn descending
                select new VentaSuspendidaResumenDto
                {
                    Id = v.Id,
                    Nombre = v.Nombre,
                    ClienteNombre = v.ClienteNombre,
                    CantidadItems = v.Items.Count,
                    Total = v.Items.Sum(i => i.Precio * i.Cantidad),
                    CreadoEn = v.CreadoEn,
                    CreadoPorNombre = u.Nombre
                }
            )
            .ToListAsync(ct);

    public async Task<VentaSuspendidaDetalleDto> ObtenerAsync(Guid sucursalId, Guid id, CancellationToken ct = default)
    {
        var venta = await db.VentasSuspendidas
            .Include(v => v.Items).ThenInclude(i => i.IngredientesExcluidos)
            .FirstOrDefaultAsync(v => v.Id == id && v.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La venta guardada no existe.");

        return new VentaSuspendidaDetalleDto
        {
            Id = venta.Id,
            Nombre = venta.Nombre,
            ClienteId = venta.ClienteId,
            ClienteNombre = venta.ClienteNombre,
            PorcentajePropina = venta.PorcentajePropina,
            MontoDescuento = venta.MontoDescuento,
            DescuentoAutorizado = venta.DescuentoAutorizado,
            Items = venta.Items.Select(i => new VentaSuspendidaItemDto
            {
                ProductoId = i.ProductoId,
                CategoriaId = i.CategoriaId,
                NombreProducto = i.NombreProducto,
                Precio = i.Precio,
                Cantidad = i.Cantidad,
                IngredientesExcluidosIds = i.IngredientesExcluidos.Select(e => e.IngredienteId).ToList(),
                NombresExcluidos = i.IngredientesExcluidos.Select(e => e.NombreIngrediente).ToList()
            }).ToList()
        };
    }

    public async Task<Guid> GuardarAsync(Guid sucursalId, Guid usuarioId, GuardarVentaSuspendidaRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre/referencia es obligatorio.");

        if (request.Items.Count == 0)
            throw new InvalidOperationException("No hay nada en el carrito para guardar.");

        var venta = new VentaSuspendida
        {
            SucursalId = sucursalId,
            TurnoCajaId = request.TurnoCajaId,
            Nombre = request.Nombre,
            ClienteId = request.ClienteId,
            ClienteNombre = request.ClienteNombre,
            PorcentajePropina = request.PorcentajePropina,
            MontoDescuento = request.MontoDescuento,
            DescuentoAutorizado = request.DescuentoAutorizado,
            CreadoPorUsuarioId = usuarioId
        };

        foreach (var item in request.Items)
        {
            var ventaItem = new VentaSuspendidaItem
            {
                ProductoId = item.ProductoId,
                CategoriaId = item.CategoriaId,
                NombreProducto = item.NombreProducto,
                Precio = item.Precio,
                Cantidad = item.Cantidad
            };

            foreach (var ingredienteId in item.IngredientesExcluidosIds)
            {
                var nombre = item.NombresExcluidos.ElementAtOrDefault(item.IngredientesExcluidosIds.IndexOf(ingredienteId)) ?? "?";
                ventaItem.IngredientesExcluidos.Add(new VentaSuspendidaItemIngrediente
                {
                    IngredienteId = ingredienteId,
                    NombreIngrediente = nombre
                });
            }

            venta.Items.Add(ventaItem);
        }

        db.VentasSuspendidas.Add(venta);
        await db.SaveChangesAsync(ct);
        return venta.Id;
    }

    public async Task EliminarAsync(Guid sucursalId, Guid id, CancellationToken ct = default)
    {
        var venta = await db.VentasSuspendidas.FirstOrDefaultAsync(v => v.Id == id && v.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La venta guardada no existe.");

        db.VentasSuspendidas.Remove(venta);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExisteAlgunaAsync(Guid sucursalId, CancellationToken ct = default) =>
        await db.VentasSuspendidas.AnyAsync(v => v.SucursalId == sucursalId, ct);
}
