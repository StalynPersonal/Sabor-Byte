using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.Facturacion.Dtos;
using SaborByte.Aplicacion.Interfaces;

namespace SaborByte.Aplicacion.Facturacion;

// Consulta de facturas ya emitidas — puramente de lectura. Las facturas SIEMPRE se
// generan desde Caja (VentaAppService); este servicio nunca crea ni modifica una.
public class FacturaAppService(IAppDbContext db)
{
    // Búsqueda paginada (por NCF o número de factura, exacto o parcial, más filtros
    // opcionales de fecha/monto/caja) — usada tanto por la pantalla "Facturas" de
    // Central como por "Emitir nota de crédito" en Caja.
    public async Task<ResultadoPaginado<FacturaResumenDto>> BuscarAsync(
        Guid sucursalId, string? texto, DateTime? desde, DateTime? hasta,
        decimal? montoMinimo, decimal? montoMaximo, Guid? cajaId,
        int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.Facturas.Where(f => f.SucursalId == sucursalId);

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(f =>
                (f.NumeroNcf != null && f.NumeroNcf.Contains(texto)) ||
                (f.NumeroFactura != null && f.NumeroFactura.Contains(texto)));

        if (desde is not null)
            query = query.Where(f => f.FechaEmision >= desde.Value);
        if (hasta is not null)
            query = query.Where(f => f.FechaEmision <= hasta.Value);
        if (montoMinimo is not null)
            query = query.Where(f => f.Total >= montoMinimo.Value);
        if (montoMaximo is not null)
            query = query.Where(f => f.Total <= montoMaximo.Value);

        var consulta =
            from f in query
            join tc in db.TurnosCaja on f.CajaTurnoId equals tc.Id
            join c in db.Cajas on tc.CajaId equals c.Id
            select new { Factura = f, Caja = c };

        if (cajaId is not null)
            consulta = consulta.Where(x => x.Caja.Id == cajaId.Value);

        var total = await consulta.CountAsync(ct);

        var items = await consulta
            .OrderByDescending(x => x.Factura.FechaEmision)
            .Select(x => new FacturaResumenDto
            {
                Id = x.Factura.Id,
                NumeroFactura = x.Factura.NumeroFactura,
                NumeroNcf = x.Factura.NumeroNcf,
                CajaNumero = x.Caja.Numero,
                Total = x.Factura.Total,
                FechaEmision = x.Factura.FechaEmision
            })
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        var codigoSucursal = await db.Sucursales.Where(s => s.Id == sucursalId).Select(s => s.Codigo).FirstOrDefaultAsync(ct);
        foreach (var item in items)
            item.SucursalCodigo = codigoSucursal;

        return new ResultadoPaginado<FacturaResumenDto>
        {
            Items = items,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = total
        };
    }

    // Detalle completo de una factura ya emitida: líneas, pagos, cliente — lo que
    // abre el icono de "ver detalle" en la pantalla "Facturas" de Central.
    public async Task<FacturaDetalleCompletoDto> ObtenerDetalleCompletoAsync(
        Guid sucursalId, Guid facturaId, CancellationToken ct = default)
    {
        var factura = await db.Facturas
            .Include(f => f.Detalle)
            .Include(f => f.Pagos).ThenInclude(p => p.MetodoPago)
            .FirstOrDefaultAsync(f => f.Id == facturaId && f.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La factura no existe.");

        var cajaNumero = await (
            from tc in db.TurnosCaja
            join c in db.Cajas on tc.CajaId equals c.Id
            where tc.Id == factura.CajaTurnoId
            select c.Numero
        ).FirstOrDefaultAsync(ct);

        var sucursal = await db.Sucursales.Where(s => s.Id == sucursalId).Select(s => new { s.Nombre, s.Codigo }).FirstOrDefaultAsync(ct);
        var cajeroNombre = await db.Usuarios.Where(u => u.Id == factura.CreadoPorUsuarioId).Select(u => u.Nombre).FirstOrDefaultAsync(ct);

        return new FacturaDetalleCompletoDto
        {
            Id = factura.Id,
            NumeroFactura = factura.NumeroFactura,
            NumeroNcf = factura.NumeroNcf,
            SucursalNombre = sucursal?.Nombre,
            SucursalCodigo = sucursal?.Codigo,
            CajaNumero = cajaNumero,
            FechaEmision = factura.FechaEmision,
            ClienteNombre = factura.ClienteNombre,
            ClienteRncOCedula = factura.ClienteRncOCedula,
            CajeroNombre = cajeroNombre,
            CodigoSeguridadDgii = factura.CodigoSeguridadDgii,
            Subtotal = factura.Subtotal,
            Itbis = factura.Itbis,
            Descuento = factura.Descuento,
            Propina = factura.Propina,
            Total = factura.Total,
            Lineas = factura.Detalle.Select(d => new FacturaLineaDetalleDto
            {
                NombreProducto = d.NombreProducto,
                Codigo = d.Codigo,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                TasaItbis = d.TasaItbis,
                Itbis = d.Itbis,
                Total = d.Total,
                CantidadAcreditada = d.CantidadAcreditada
            }).ToList(),
            Pagos = factura.Pagos.Select(p => new FacturaPagoDetalleDto
            {
                NombreMetodoPago = p.MetodoPago?.Nombre ?? "?",
                Monto = p.Monto,
                NumeroComprobante = p.NumeroComprobante
            }).ToList()
        };
    }
}
