using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.Facturacion.Dtos;
using SaborByte.Aplicacion.Identidad;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Facturacion;

namespace SaborByte.Aplicacion.Facturacion;

// Nombre de la acción que se pasa a AutorizacionAppService — toda emisión de nota de
// crédito requiere un código de autorización de Supervisor/Admin fresco, igual que un
// descuento en VentaAppService, sin importar el rol del usuario que está logueado.
public class NotaCreditoAppService(IAppDbContext db, IAuditoriaService auditoria, AutorizacionAppService autorizacion)
{
    private const string AccionAutorizacion = "EmitirNotaCredito";

    // Líneas de la factura original con lo que todavía se puede acreditar — lo que
    // alimenta el selector de "qué y cuánto" al armar una nota.
    public async Task<List<FacturaDetalleDisponibleDto>> ObtenerDetalleDisponibleAsync(
        Guid sucursalId, Guid facturaId, CancellationToken ct = default)
    {
        var perteneceASucursal = await db.Facturas.AnyAsync(f => f.Id == facturaId && f.SucursalId == sucursalId, ct);
        if (!perteneceASucursal)
            throw new InvalidOperationException("La factura no existe.");

        return await db.FacturaDetalles
            .Where(d => d.FacturaId == facturaId)
            .Select(d => new FacturaDetalleDisponibleDto
            {
                FacturaDetalleId = d.Id,
                NombreProducto = d.NombreProducto,
                Codigo = d.Codigo,
                Cantidad = d.Cantidad,
                CantidadAcreditada = d.CantidadAcreditada,
                PrecioUnitario = d.PrecioUnitario,
                Total = d.Total
            })
            .ToListAsync(ct);
    }

    public async Task<NotaCreditoDto> CrearAsync(
        Guid sucursalId, Guid usuarioId, CrearNotaRequestDto request, CancellationToken ct = default)
    {
        var facturaOriginal = await db.Facturas.FirstOrDefaultAsync(
            f => f.Id == request.FacturaOriginalId && f.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La factura original no existe.");

        if (request.Detalle.Count == 0)
            throw new InvalidOperationException("La nota debe incluir al menos una línea de la factura original.");

        var motivo = await db.MotivosNotaCredito.FirstOrDefaultAsync(m => m.Id == request.MotivoId && m.Activo, ct)
            ?? throw new InvalidOperationException("El motivo no existe o está inactivo.");

        // Nota: toda emisión de nota de crédito exige autorización de un Supervisor/Admin
        // (mismo flujo que un descuento en Caja) — no se emiten notas de débito en el
        // sistema, así que el tipo no viene del cliente, siempre es Crédito.
        await autorizacion.ValidarYConsumirAsync(request.CodigoAutorizacion, AccionAutorizacion, ct);

        var detalleIds = request.Detalle.Select(d => d.FacturaDetalleId).ToList();
        var lineasFactura = await db.FacturaDetalles
            .Where(d => detalleIds.Contains(d.Id) && d.FacturaId == facturaOriginal.Id)
            .ToDictionaryAsync(d => d.Id, ct);

        var nota = new NotaCredito
        {
            SucursalId = sucursalId,
            FacturaOriginalId = facturaOriginal.Id,
            MotivoId = motivo.Id,
            Motivo = motivo.Nombre,
            CreadoPorUsuarioId = usuarioId
        };

        decimal montoTotal = 0;

        foreach (var lineaSolicitada in request.Detalle)
        {
            if (!lineasFactura.TryGetValue(lineaSolicitada.FacturaDetalleId, out var lineaFactura))
                throw new InvalidOperationException("Una de las líneas seleccionadas no pertenece a esta factura.");

            if (lineaSolicitada.Cantidad <= 0)
                throw new InvalidOperationException($"La cantidad a acreditar de '{lineaFactura.NombreProducto}' debe ser mayor a cero.");

            var disponible = lineaFactura.Cantidad - lineaFactura.CantidadAcreditada;
            if (lineaSolicitada.Cantidad > disponible)
                throw new InvalidOperationException(
                    $"'{lineaFactura.NombreProducto}': solo quedan {disponible:0.###} sin acreditar de {lineaFactura.Cantidad:0.###}.");

            // Precio efectivo por unidad (incluye impuesto/descuento ya prorrateados en Total),
            // para que acreditar 1 de 2 unidades sea proporcional, no la mitad "a ojo".
            var precioEfectivoUnitario = lineaFactura.Cantidad == 0 ? 0 : lineaFactura.Total / lineaFactura.Cantidad;
            var montoLinea = Math.Round(precioEfectivoUnitario * lineaSolicitada.Cantidad, 2);

            nota.Detalle.Add(new NotaCreditoDetalle
            {
                NotaCreditoId = nota.Id,
                FacturaDetalleId = lineaFactura.Id,
                Cantidad = lineaSolicitada.Cantidad,
                Monto = montoLinea
            });

            lineaFactura.CantidadAcreditada += lineaSolicitada.Cantidad;
            montoTotal += montoLinea;
        }

        nota.Monto = montoTotal;

        var ecfActivo = await db.Sucursales.Where(s => s.Id == sucursalId).Select(s => s.EcfActivo).FirstAsync(ct);

        await AsignarNumeroNotaAsync(sucursalId, nota, ct);
        await AsignarNcfSiAplicaAsync(sucursalId, ecfActivo, nota, ct);

        db.NotasCredito.Add(nota);
        await db.SaveChangesAsync(ct);

        await auditoria.RegistrarAsync(sucursalId, usuarioId, "EmitirCredito", "NotaCredito", nota.Id,
            $"Factura original: {facturaOriginal.NumeroFactura}; Motivo: {motivo.Nombre}; Monto: {nota.Monto:0.00}", ct);

        var (sucursalCodigo, cajaNumero) = await ObtenerCodigosDeFacturaAsync(facturaOriginal.CajaTurnoId, sucursalId, ct);
        var cajeroNombre = await db.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Nombre).FirstOrDefaultAsync(ct);

        return new NotaCreditoDto
        {
            Id = nota.Id,
            FacturaOriginalId = nota.FacturaOriginalId,
            NumeroNota = nota.NumeroNota,
            NumeroFacturaOriginal = facturaOriginal.NumeroFactura,
            NumeroNcf = nota.NumeroNcf,
            Motivo = nota.Motivo,
            SucursalCodigo = sucursalCodigo,
            CajaNumero = cajaNumero,
            Monto = nota.Monto,
            FechaEmision = nota.FechaEmision,
            ClienteNombre = facturaOriginal.ClienteNombre,
            ClienteRncOCedula = facturaOriginal.ClienteRncOCedula,
            CajeroNombre = cajeroNombre,
            Detalle = nota.Detalle.Select(d => new NotaCreditoDetalleDto
            {
                FacturaDetalleId = d.FacturaDetalleId,
                NombreProducto = lineasFactura[d.FacturaDetalleId].NombreProducto,
                Cantidad = d.Cantidad,
                Monto = d.Monto
            }).ToList()
        };
    }

    // Listado paginado de TODAS las notas de la sucursal — para la pantalla de solo
    // lectura "Notas de Crédito" en Central (ya no se emiten desde ahí, solo se consultan).
    public async Task<ResultadoPaginado<NotaCreditoDto>> ListarAsync(
        Guid sucursalId, string? texto, DateTime? desde, DateTime? hasta,
        decimal? montoMinimo, decimal? montoMaximo, Guid? cajaId,
        int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.NotasCredito
            .Include(n => n.Detalle).ThenInclude(d => d.FacturaDetalle)
            .Where(n => n.SucursalId == sucursalId);

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(n =>
                (n.NumeroNota != null && n.NumeroNota.Contains(texto)) ||
                (n.NumeroNcf != null && n.NumeroNcf.Contains(texto)));

        if (desde is not null)
            query = query.Where(n => n.FechaEmision >= desde.Value);
        if (hasta is not null)
            query = query.Where(n => n.FechaEmision <= hasta.Value);
        if (montoMinimo is not null)
            query = query.Where(n => n.Monto >= montoMinimo.Value);
        if (montoMaximo is not null)
            query = query.Where(n => n.Monto <= montoMaximo.Value);

        // El filtro por caja se aplica sobre la caja de la FACTURA ORIGINAL (la nota en
        // sí no tiene caja propia — se emite desde Central/Caja sin turno asociado).
        if (cajaId is not null)
        {
            var facturaIdsDeCaja = await (
                from f in db.Facturas
                join tc in db.TurnosCaja on f.CajaTurnoId equals tc.Id
                where tc.CajaId == cajaId.Value
                select f.Id
            ).ToListAsync(ct);
            query = query.Where(n => facturaIdsDeCaja.Contains(n.FacturaOriginalId));
        }

        var total = await query.CountAsync(ct);

        var notas = await query
            .OrderByDescending(n => n.FechaEmision)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        var facturaIds = notas.Select(n => n.FacturaOriginalId).Distinct().ToList();
        var facturas = await db.Facturas
            .Where(f => facturaIds.Contains(f.Id))
            .Select(f => new { f.Id, f.NumeroFactura, f.CajaTurnoId })
            .ToListAsync(ct);
        var facturasPorId = facturas.ToDictionary(f => f.Id);

        var codigoSucursal = await db.Sucursales.Where(s => s.Id == sucursalId).Select(s => s.Codigo).FirstOrDefaultAsync(ct);

        var cajaTurnoIds = facturas.Select(f => f.CajaTurnoId).Distinct().ToList();
        var cajaNumeroPorTurno = await (
            from tc in db.TurnosCaja
            join c in db.Cajas on tc.CajaId equals c.Id
            where cajaTurnoIds.Contains(tc.Id)
            select new { tc.Id, c.Numero }
        ).ToDictionaryAsync(x => x.Id, x => x.Numero, ct);

        var items = notas.Select(n =>
        {
            facturasPorId.TryGetValue(n.FacturaOriginalId, out var factura);
            var cajaNumero = factura is not null && cajaNumeroPorTurno.TryGetValue(factura.CajaTurnoId, out var num) ? num : null;

            return new NotaCreditoDto
            {
                Id = n.Id,
                FacturaOriginalId = n.FacturaOriginalId,
                NumeroNota = n.NumeroNota,
                NumeroFacturaOriginal = factura?.NumeroFactura,
                NumeroNcf = n.NumeroNcf,
                Motivo = n.Motivo,
                SucursalCodigo = codigoSucursal,
                CajaNumero = cajaNumero,
                Monto = n.Monto,
                FechaEmision = n.FechaEmision,
                Detalle = n.Detalle.Select(d => new NotaCreditoDetalleDto
                {
                    FacturaDetalleId = d.FacturaDetalleId,
                    NombreProducto = d.FacturaDetalle?.NombreProducto ?? "",
                    Cantidad = d.Cantidad,
                    Monto = d.Monto
                }).ToList()
            };
        }).ToList();

        return new ResultadoPaginado<NotaCreditoDto>
        {
            Items = items,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = total
        };
    }

    public async Task<List<NotaCreditoDto>> ListarPorFacturaAsync(Guid sucursalId, Guid facturaId, CancellationToken ct = default)
    {
        var factura = await db.Facturas.FirstOrDefaultAsync(f => f.Id == facturaId, ct);

        var notas = await db.NotasCredito
            .Include(n => n.Detalle).ThenInclude(d => d.FacturaDetalle)
            .Where(n => n.FacturaOriginalId == facturaId && n.SucursalId == sucursalId)
            .OrderByDescending(n => n.FechaEmision)
            .ToListAsync(ct);

        var (sucursalCodigo, cajaNumero) = factura is null
            ? (null, null)
            : await ObtenerCodigosDeFacturaAsync(factura.CajaTurnoId, sucursalId, ct);

        return notas.Select(n => new NotaCreditoDto
        {
            Id = n.Id,
            FacturaOriginalId = n.FacturaOriginalId,
            NumeroNota = n.NumeroNota,
            NumeroFacturaOriginal = factura?.NumeroFactura,
            NumeroNcf = n.NumeroNcf,
            Motivo = n.Motivo,
            SucursalCodigo = sucursalCodigo,
            CajaNumero = cajaNumero,
            Monto = n.Monto,
            FechaEmision = n.FechaEmision,
            Detalle = n.Detalle.Select(d => new NotaCreditoDetalleDto
            {
                FacturaDetalleId = d.FacturaDetalleId,
                NombreProducto = d.FacturaDetalle?.NombreProducto ?? "",
                Cantidad = d.Cantidad,
                Monto = d.Monto
            }).ToList()
        }).ToList();
    }

    // La nota no tiene caja propia — se hereda de la caja que emitió la factura original,
    // para que se vea de dónde salió (misma trazabilidad que Factura.NumeroFactura).
    private async Task<(string? SucursalCodigo, string? CajaNumero)> ObtenerCodigosDeFacturaAsync(
        Guid cajaTurnoId, Guid sucursalId, CancellationToken ct)
    {
        var cajaNumero = await (
            from tc in db.TurnosCaja
            join c in db.Cajas on tc.CajaId equals c.Id
            where tc.Id == cajaTurnoId
            select c.Numero
        ).FirstOrDefaultAsync(ct);

        var sucursalCodigo = await db.Sucursales.Where(s => s.Id == sucursalId).Select(s => s.Codigo).FirstOrDefaultAsync(ct);

        return (sucursalCodigo, cajaNumero);
    }

    // Número interno de la nota (no fiscal), siempre asignado: CodigoSucursal(2) +
    // "33"(2) + Secuencia(5) = 9 dígitos — mismo patrón CAS que Factura.NumeroFactura,
    // pero el contador vive en Sucursal (las notas no tienen caja).
    private async Task AsignarNumeroNotaAsync(Guid sucursalId, NotaCredito nota, CancellationToken ct)
    {
        var sucursal = await db.Sucursales.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId, ct)
            ?? throw new InvalidOperationException("La sucursal no existe.");

        if (string.IsNullOrWhiteSpace(sucursal.Codigo))
            throw new InvalidOperationException("La sucursal no tiene código configurado (2 dígitos); no se puede generar el número de la nota.");

        const string tipoInterno = "33"; // e-CF 33 = Nota de Crédito (DGII)

        while (true)
        {
            var actual = await db.Sucursales.AsNoTracking()
                .Where(s => s.Id == sucursalId)
                .Select(s => s.ProximoNumeroNota)
                .FirstAsync(ct);

            var filasActualizadas = await db.Sucursales
                .Where(s => s.Id == sucursalId && s.ProximoNumeroNota == actual)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProximoNumeroNota, x => x.ProximoNumeroNota + 1), ct);

            if (filasActualizadas == 0)
                continue; // otra nota concurrente ya reservó este número; reintentar con el valor actualizado

            nota.NumeroNota = $"{sucursal.Codigo}{tipoInterno}{actual:D5}";
            return;
        }
    }

    // Igual que la factura original (ver VentaAppService.AsignarNcfSiAplicaAsync): si la
    // sucursal no tiene "Facturación electrónica (e-CF/DGII)" activada, nunca se asigna
    // NCF, aunque exista una secuencia vigente para el tipo 33 — el checkbox de la
    // sucursal es el interruptor único.
    private async Task AsignarNcfSiAplicaAsync(Guid sucursalId, bool ecfActivo, NotaCredito nota, CancellationToken ct)
    {
        const string tipoComprobante = "33"; // e-CF 33 = Nota de Crédito (DGII)

        if (!ecfActivo)
        {
            nota.EstadoDgii = EstadoDgii.NoAplica;
            return;
        }

        var secuencia = await db.SecuenciasNcf.FirstOrDefaultAsync(s =>
            s.SucursalId == sucursalId &&
            s.TipoComprobante == tipoComprobante &&
            s.Activa &&
            s.FechaVencimiento > DateTime.UtcNow &&
            s.SecuenciaProxima <= s.SecuenciaFinal, ct);

        if (secuencia is null)
        {
            nota.EstadoDgii = EstadoDgii.NoAplica;
            return;
        }

        nota.NumeroNcf = secuencia.FormatearNumero(secuencia.SecuenciaProxima);
        nota.TipoComprobante = secuencia.TipoComprobante;
        nota.EstadoDgii = EstadoDgii.NoAplica;
        secuencia.SecuenciaProxima++;
    }
}
