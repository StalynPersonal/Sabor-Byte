using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Facturacion.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Caja;
using SaborByte.Dominio.Facturacion;

namespace SaborByte.Aplicacion.Facturacion;

public class VentaAppService(
    IAppDbContext db,
    Inventario.InventarioAppService inventario,
    Identidad.AutorizacionAppService autorizacion,
    IAuditoriaService auditoria)
{
    private const decimal TasaItbis = 0.18m;

    public async Task<VentaResultadoDto> CrearVentaAsync(
        Guid sucursalId, Guid usuarioId, CrearVentaRequestDto request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("La venta debe tener al menos un producto.");

        // Descuentos solo con autorización de Supervisor/Admin (sección 7 del plan).
        if (request.Items.Any(i => i.Descuento > 0))
        {
            if (request.CodigoAutorizacionDescuento is null)
                throw new InvalidOperationException("El descuento requiere autorización de un Supervisor o Administrador.");

            await autorizacion.ValidarYConsumirAsync(request.CodigoAutorizacionDescuento.Value, "Descuento", ct);
            await auditoria.RegistrarAsync(sucursalId, usuarioId, "Descuento", "Factura",
                detalle: $"Monto descuento: {request.Items.Sum(i => i.Descuento):0.00}", ct: ct);
        }

        var turno = await db.TurnosCaja.FirstOrDefaultAsync(t => t.Id == request.TurnoCajaId, ct)
            ?? throw new InvalidOperationException("El turno de caja no existe.");

        if (turno.Estado != EstadoTurnoCaja.Abierto)
            throw new InvalidOperationException("No se puede facturar sobre un turno de caja cerrado.");

        var productoIds = request.Items.Select(i => i.ProductoId).ToList();
        var productos = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var factura = new Factura
        {
            SucursalId = sucursalId,
            CajaTurnoId = turno.Id,
            ClienteId = request.ClienteId,
            CreadoPorUsuarioId = usuarioId,
            FechaEmision = DateTime.UtcNow
        };

        decimal subtotal = 0, itbis = 0, descuentoTotal = 0;

        foreach (var item in request.Items)
        {
            if (!productos.TryGetValue(item.ProductoId, out var producto))
                throw new InvalidOperationException($"El producto {item.ProductoId} no existe.");

            var totalLinea = (producto.Precio * item.Cantidad) - item.Descuento;
            var itbisLinea = producto.AplicaItbis ? Math.Round(totalLinea * TasaItbis, 2) : 0m;

            subtotal += producto.Precio * item.Cantidad;
            descuentoTotal += item.Descuento;
            itbis += itbisLinea;

            factura.Detalle.Add(new FacturaDetalle
            {
                FacturaId = factura.Id,
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                Descuento = item.Descuento,
                Itbis = itbisLinea,
                Total = totalLinea + itbisLinea
            });
        }

        var propina = request.MontoPropinaFijo ?? (request.PorcentajePropina is > 0
            ? Math.Round((subtotal - descuentoTotal + itbis) * (request.PorcentajePropina.Value / 100m), 2)
            : 0m);

        factura.Subtotal = subtotal;
        factura.Descuento = descuentoTotal;
        factura.Itbis = itbis;
        factura.Propina = propina;
        factura.Total = subtotal - descuentoTotal + itbis + propina;

        await AsignarNcfSiAplicaAsync(sucursalId, factura, ct);

        db.Facturas.Add(factura);

        db.MovimientosCaja.Add(new MovimientoCaja
        {
            TurnoCajaId = turno.Id,
            Tipo = TipoMovimientoCaja.Venta,
            FacturaId = factura.Id,
            FormaPago = request.FormaPago,
            Monto = factura.Total,
            Descripcion = $"Venta {factura.NumeroNcf ?? factura.Id.ToString()[..8]}"
        });

        foreach (var item in request.Items)
        {
            await inventario.DescontarPorVentaAsync(
                sucursalId, item.ProductoId, item.Cantidad, factura.Id,
                item.IngredientesExcluidosIds, usuarioId, ct);
        }

        await db.SaveChangesAsync(ct);

        return new VentaResultadoDto
        {
            FacturaId = factura.Id,
            NumeroNcf = factura.NumeroNcf,
            Subtotal = factura.Subtotal,
            Itbis = factura.Itbis,
            Descuento = factura.Descuento,
            Propina = factura.Propina,
            Total = factura.Total,
            FechaEmision = factura.FechaEmision
        };
    }

    // v1: sin e-CF (Fase 4 pendiente). Si hay una secuencia NCF tradicional activa, se asigna;
    // si no, la factura queda "sin NCF" (consumo interno), sin bloquear la venta.
    private async Task AsignarNcfSiAplicaAsync(Guid sucursalId, Factura factura, CancellationToken ct)
    {
        var secuencia = await db.SecuenciasNcf.FirstOrDefaultAsync(s =>
            s.SucursalId == sucursalId &&
            s.Activa &&
            s.FechaVencimiento > DateTime.UtcNow &&
            s.SecuenciaProxima <= s.SecuenciaFinal, ct);

        if (secuencia is null)
        {
            factura.EstadoDgii = EstadoDgii.NoAplica;
            return;
        }

        factura.NumeroNcf = secuencia.FormatearNumero(secuencia.SecuenciaProxima);
        factura.TipoComprobante = secuencia.TipoComprobante;
        factura.EstadoDgii = EstadoDgii.NoAplica; // no es e-CF, es NCF tradicional
        secuencia.SecuenciaProxima++;
    }
}
