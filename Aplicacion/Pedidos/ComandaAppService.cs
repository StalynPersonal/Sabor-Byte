using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Pedidos.Dtos;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Pedidos;

namespace SaborByte.Aplicacion.Pedidos;

public class ComandaAppService(
    IAppDbContext db,
    Inventario.InventarioAppService inventario,
    INotificadorComandas notificador,
    IAuditoriaService auditoria)
{
    public async Task<ComandaDto> CrearComandaAsync(
        Guid sucursalId, Guid? usuarioMeseroId, CrearComandaRequestDto request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("La comanda debe tener al menos un producto.");

        var productoIds = request.Items.Select(i => i.ProductoId).ToList();
        var productos = await db.Productos
            .Where(p => productoIds.Contains(p.Id) && p.TipoProducto == TipoProducto.Vendible)
            .ToDictionaryAsync(p => p.Id, ct);

        var comanda = new Comanda
        {
            SucursalId = sucursalId,
            MesaId = request.MesaId,
            MeseroId = request.MeseroId ?? usuarioMeseroId
        };

        foreach (var item in request.Items)
        {
            if (!productos.TryGetValue(item.ProductoId, out var producto))
                throw new InvalidOperationException($"El producto {item.ProductoId} no existe o no es vendible.");

            var comandaItem = new ComandaItem
            {
                ComandaId = comanda.Id,
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                Notas = item.Notas
            };

            foreach (var ingredienteId in item.IngredientesExcluidosIds)
                comandaItem.IngredientesExcluidos.Add(new ComandaItemIngrediente { IngredienteId = ingredienteId });

            comanda.Items.Add(comandaItem);
        }

        comanda.Estado = EstadoComanda.EnviadaCocina;
        db.Comandas.Add(comanda);

        if (request.MesaId is not null)
        {
            var mesa = await db.Mesas.FirstOrDefaultAsync(m => m.Id == request.MesaId && m.SucursalId == sucursalId, ct)
                ?? throw new InvalidOperationException("La mesa no existe.");
            mesa.Estado = EstadoMesa.Ocupada;
        }

        await db.SaveChangesAsync(ct); // asigna NumeroComanda (identity)

        // Descontar inventario al enviar a cocina (ver sección 5 del plan).
        foreach (var item in comanda.Items)
        {
            await inventario.DescontarPorVentaAsync(
                sucursalId, item.ProductoId, item.Cantidad, item.Id,
                item.IngredientesExcluidos.Select(i => i.IngredienteId).ToList(),
                usuarioMeseroId, ct);
            item.InventarioDescontado = true;
        }

        await db.SaveChangesAsync(ct);

        var dto = MapearComanda(comanda);
        await notificador.ComandaCreadaAsync(sucursalId, dto);
        return dto;
    }

    public async Task<List<ComandaDto>> ObtenerAbiertasAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var comandas = await db.Comandas
            .Include(c => c.Items).ThenInclude(i => i.IngredientesExcluidos)
            .Where(c => c.SucursalId == sucursalId &&
                        (c.Estado == EstadoComanda.Abierta || c.Estado == EstadoComanda.EnviadaCocina))
            .OrderBy(c => c.CreadoEn)
            .ToListAsync(ct);

        return comandas.Select(MapearComanda).ToList();
    }

    public async Task<ComandaItemDto> CambiarEstadoItemAsync(
        Guid sucursalId, Guid comandaItemId, EstadoItemComanda nuevoEstado, CancellationToken ct = default)
    {
        // Evita IDOR: sucursalId viene del query string del cliente; sin este filtro,
        // un usuario con acceso legítimo a SU sucursal podía pasar el sucursalId de esa
        // sucursal (que sí supera TieneAccesoASucursal) junto al comandaItemId de OTRA
        // sucursal, y modificar/cancelar comandas ajenas.
        var item = await db.ComandaItems
            .Include(i => i.IngredientesExcluidos)
            .Include(i => i.Comanda)
            .FirstOrDefaultAsync(i => i.Id == comandaItemId && i.Comanda!.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El ítem de comanda no existe.");

        ValidarTransicion(item.Estado, nuevoEstado);
        item.Estado = nuevoEstado;
        await db.SaveChangesAsync(ct);

        var dto = MapearItem(item);
        await notificador.ItemComandaActualizadoAsync(sucursalId, item.ComandaId, dto);

        var todosListos = await db.ComandaItems
            .Where(i => i.ComandaId == item.ComandaId)
            .AllAsync(i => i.Estado == EstadoItemComanda.Listo || i.Estado == EstadoItemComanda.Entregado ||
                           i.Estado == EstadoItemComanda.Cancelado, ct);

        if (todosListos)
            await notificador.ComandaListaParaEntregaAsync(sucursalId, item.ComandaId);

        return dto;
    }

    public async Task CancelarItemAsync(
        Guid sucursalId, Guid comandaItemId, Guid usuarioId, CancelarItemRequestDto request, CancellationToken ct = default)
    {
        // Mismo control anti-IDOR que CambiarEstadoItemAsync (ver comentario ahí).
        var item = await db.ComandaItems
            .Include(i => i.IngredientesExcluidos)
            .Include(i => i.Comanda)
            .FirstOrDefaultAsync(i => i.Id == comandaItemId && i.Comanda!.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El ítem de comanda no existe.");

        if (item.Estado == EstadoItemComanda.Entregado)
            throw new InvalidOperationException("No se puede cancelar un ítem ya entregado.");

        item.Estado = EstadoItemComanda.Cancelado;

        var inventarioRevertido = false;
        if (item.InventarioDescontado)
        {
            await inventario.RevertirPorCancelacionAsync(
                sucursalId, item.ProductoId, item.Cantidad, item.Id,
                item.IngredientesExcluidos.Select(i => i.IngredienteId).ToList(),
                usuarioId, ct);
            inventarioRevertido = true;
        }

        db.ComandaCancelaciones.Add(new Dominio.Pedidos.ComandaCancelacion
        {
            ComandaId = item.ComandaId,
            ComandaItemId = item.Id,
            CanceladoPorUsuarioId = usuarioId,
            RolQueCancelo = request.Rol,
            Motivo = request.Motivo,
            InventarioRevertido = inventarioRevertido
        });

        await db.SaveChangesAsync(ct);
        await notificador.ComandaCanceladaAsync(sucursalId, item.ComandaId, item.Id);
        await auditoria.RegistrarAsync(sucursalId, usuarioId, "CancelacionItemComanda", "ComandaItem", item.Id,
            $"Motivo: {request.Motivo}; Rol: {request.Rol}; InventarioRevertido: {inventarioRevertido}", ct);
    }

    // Cancela la comanda completa (a diferencia de CancelarItemAsync, que cancela un solo
    // ítem) — ver sección 5 del plan: "Cualquier ComandaItem (o la Comanda completa) puede
    // pasar a Cancelado". Los ítems ya Entregados no se tocan (no se puede deshacer una
    // entrega); si TODOS los ítems ya están Entregados o Cancelados, no hay nada que cancelar.
    public async Task CancelarComandaAsync(
        Guid sucursalId, Guid comandaId, Guid usuarioId, CancelarComandaRequestDto request, CancellationToken ct = default)
    {
        var comanda = await db.Comandas
            .Include(c => c.Items).ThenInclude(i => i.IngredientesExcluidos)
            .FirstOrDefaultAsync(c => c.Id == comandaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La comanda no existe.");

        if (comanda.Estado is EstadoComanda.Cerrada or EstadoComanda.Cancelada)
            throw new InvalidOperationException($"No se puede cancelar una comanda en estado '{comanda.Estado}'.");

        var itemsACancelar = comanda.Items
            .Where(i => i.Estado is not (EstadoItemComanda.Entregado or EstadoItemComanda.Cancelado))
            .ToList();

        if (itemsACancelar.Count == 0)
            throw new InvalidOperationException("Todos los ítems de la comanda ya están entregados o cancelados.");

        foreach (var item in itemsACancelar)
        {
            item.Estado = EstadoItemComanda.Cancelado;

            if (item.InventarioDescontado)
            {
                await inventario.RevertirPorCancelacionAsync(
                    sucursalId, item.ProductoId, item.Cantidad, item.Id,
                    item.IngredientesExcluidos.Select(e => e.IngredienteId).ToList(),
                    usuarioId, ct);
            }
        }

        // Si quedan ítems ya entregados, la comanda queda Cerrada (esos ítems sí se
        // facturan/facturaron); si no quedaba ninguno entregado, la comanda completa se cancela.
        var quedaAlgoEntregado = comanda.Items.Any(i => i.Estado == EstadoItemComanda.Entregado);
        comanda.Estado = quedaAlgoEntregado ? EstadoComanda.Cerrada : EstadoComanda.Cancelada;

        if (comanda.Estado == EstadoComanda.Cancelada && comanda.MesaId is not null)
        {
            var mesa = await db.Mesas.FirstOrDefaultAsync(m => m.Id == comanda.MesaId, ct);
            if (mesa is not null)
                mesa.Estado = EstadoMesa.Libre;
        }

        db.ComandaCancelaciones.Add(new ComandaCancelacion
        {
            ComandaId = comanda.Id,
            ComandaItemId = null, // null = se canceló la comanda completa, no un ítem puntual
            CanceladoPorUsuarioId = usuarioId,
            RolQueCancelo = request.Rol,
            Motivo = request.Motivo,
            InventarioRevertido = itemsACancelar.Any(i => i.InventarioDescontado)
        });

        await db.SaveChangesAsync(ct);
        await notificador.ComandaCanceladaAsync(sucursalId, comanda.Id, comandaItemId: null);
        await auditoria.RegistrarAsync(sucursalId, usuarioId, "CancelacionComanda", "Comanda", comanda.Id,
            $"Motivo: {request.Motivo}; Rol: {request.Rol}; ÍtemsCancelados: {itemsACancelar.Count}", ct);
    }

    private static void ValidarTransicion(EstadoItemComanda actual, EstadoItemComanda nuevo)
    {
        var permitido = (actual, nuevo) switch
        {
            (EstadoItemComanda.Pendiente, EstadoItemComanda.EnPreparacion) => true,
            (EstadoItemComanda.EnPreparacion, EstadoItemComanda.Listo) => true,
            (EstadoItemComanda.Listo, EstadoItemComanda.Entregado) => true,
            _ => false
        };

        if (!permitido)
            throw new InvalidOperationException($"No se puede pasar un ítem de '{actual}' a '{nuevo}'.");
    }

    private static ComandaDto MapearComanda(Comanda c) => new()
    {
        Id = c.Id,
        NumeroComanda = c.NumeroComanda,
        MesaId = c.MesaId,
        MeseroId = c.MeseroId,
        Estado = c.Estado,
        CreadoEn = c.CreadoEn,
        Items = c.Items.Select(MapearItem).ToList()
    };

    private static ComandaItemDto MapearItem(ComandaItem i) => new()
    {
        Id = i.Id,
        ProductoId = i.ProductoId,
        NombreProducto = i.NombreProducto,
        Cantidad = i.Cantidad,
        PrecioUnitario = i.PrecioUnitario,
        Estado = i.Estado,
        Notas = i.Notas,
        IngredientesExcluidosIds = i.IngredientesExcluidos.Select(e => e.IngredienteId).ToList()
    };
}
