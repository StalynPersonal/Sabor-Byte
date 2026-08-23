using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Caja.Dtos;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Caja;

namespace SaborByte.Aplicacion.Caja;

public class CajaAppService(IAppDbContext db, IAuditoriaService auditoria)
{
    public async Task<List<CajaResumenDto>> ListarCajasAsync(Guid sucursalId, CancellationToken ct = default) =>
        await db.Cajas
            .Where(c => c.SucursalId == sucursalId && c.Activa)
            .Select(c => new CajaResumenDto { Id = c.Id, Numero = c.Numero, Activa = c.Activa, ProximoNumeroFactura = c.ProximoNumeroFactura, CodigoSucursal = c.CodigoSucursal })
            .ToListAsync(ct);

    // --- CRUD de cajas (Admin): incluye inactivas y expone ProximoNumeroFactura para
    // poder configurar/corregir la secuencia del número de factura interno de cada caja.
    public async Task<List<CajaDto>> ListarTodasAsync(Guid sucursalId, CancellationToken ct = default) =>
        await db.Cajas
            .Where(c => c.SucursalId == sucursalId)
            .OrderBy(c => c.Numero)
            .Select(c => MapearDto(c))
            .ToListAsync(ct);

    public async Task<Guid> CrearCajaAsync(Guid sucursalId, Guid usuarioId, GuardarCajaRequestDto request, CancellationToken ct = default)
    {
        var numeroEnUso = await db.Cajas.AnyAsync(c => c.SucursalId == sucursalId && c.Numero == request.Numero, ct);
        if (numeroEnUso)
            throw new InvalidOperationException($"Ya existe una caja con el número '{request.Numero}' en esta sucursal.");

        var codigoSucursal = await db.Sucursales.Where(s => s.Id == sucursalId).Select(s => s.Codigo).FirstOrDefaultAsync(ct);

        var caja = new Dominio.Caja.Caja
        {
            SucursalId = sucursalId,
            CodigoSucursal = codigoSucursal,
            Numero = request.Numero,
            Activa = request.Activa,
            IpPermitida = request.IpPermitida,
            HostnamePermitido = request.HostnamePermitido,
            ProximoNumeroFactura = request.ProximoNumeroFactura < 1 ? 1 : request.ProximoNumeroFactura,
            CreadoPorUsuarioId = usuarioId
        };

        db.Cajas.Add(caja);
        await db.SaveChangesAsync(ct);
        return caja.Id;
    }

    public async Task ActualizarCajaAsync(Guid sucursalId, Guid cajaId, Guid usuarioId, GuardarCajaRequestDto request, CancellationToken ct = default)
    {
        var caja = await db.Cajas.FirstOrDefaultAsync(c => c.Id == cajaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La caja no existe.");

        var numeroEnUso = await db.Cajas.AnyAsync(c => c.Id != cajaId && c.SucursalId == sucursalId && c.Numero == request.Numero, ct);
        if (numeroEnUso)
            throw new InvalidOperationException($"Ya existe otra caja con el número '{request.Numero}' en esta sucursal.");

        // Nunca se permite retroceder la secuencia: bajarla colisionaría con números de
        // factura que esa caja ya emitió (el índice único de Factura.NumeroFactura lo
        // rechazaría en producción, pero es mejor evitar la venta fallida y avisar aquí).
        if (request.ProximoNumeroFactura < caja.ProximoNumeroFactura)
            throw new InvalidOperationException(
                $"El próximo número de factura no puede retroceder (actual: {caja.ProximoNumeroFactura}). Solo puede mantenerse o avanzar.");

        caja.CodigoSucursal = await db.Sucursales.Where(s => s.Id == sucursalId).Select(s => s.Codigo).FirstOrDefaultAsync(ct);
        caja.Numero = request.Numero;
        caja.Activa = request.Activa;
        caja.IpPermitida = request.IpPermitida;
        caja.HostnamePermitido = request.HostnamePermitido;
        caja.ProximoNumeroFactura = request.ProximoNumeroFactura;
        caja.ActualizadoEn = DateTime.UtcNow;
        caja.ActualizadoPorUsuarioId = usuarioId;

        await db.SaveChangesAsync(ct);
    }

    private static CajaDto MapearDto(Dominio.Caja.Caja c) => new()
    {
        Id = c.Id,
        Numero = c.Numero,
        Activa = c.Activa,
        IpPermitida = c.IpPermitida,
        HostnamePermitido = c.HostnamePermitido,
        ProximoNumeroFactura = c.ProximoNumeroFactura
    };

    public async Task<Guid> AbrirTurnoAsync(
        Guid usuarioId, IReadOnlyCollection<Guid> sucursalesPermitidas, AbrirTurnoRequestDto request, CancellationToken ct = default)
    {
        var caja = await db.Cajas.FirstOrDefaultAsync(c => c.Id == request.CajaId, ct)
            ?? throw new InvalidOperationException("La caja no existe.");

        if (!sucursalesPermitidas.Contains(caja.SucursalId))
            throw new InvalidOperationException("La caja no existe.");

        if (!caja.Activa)
            throw new InvalidOperationException("La caja está inactiva.");

        // Seguridad: la caja solo se abre desde la máquina configurada (si se configuró).
        if (!string.IsNullOrWhiteSpace(caja.IpPermitida) && caja.IpPermitida != request.IpOrigen)
            throw new InvalidOperationException("Esta caja no está autorizada para abrirse desde esta máquina (IP no coincide).");

        if (!string.IsNullOrWhiteSpace(caja.HostnamePermitido) &&
            !string.Equals(caja.HostnamePermitido, request.HostnameOrigen, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Esta caja no está autorizada para abrirse desde esta máquina (hostname no coincide).");

        var yaHayTurnoAbierto = await db.TurnosCaja.AnyAsync(t =>
            t.CajaId == caja.Id && t.Estado == EstadoTurnoCaja.Abierto, ct);

        if (yaHayTurnoAbierto)
            throw new InvalidOperationException("Ya existe un turno abierto en esta caja. Debe cerrarse antes de abrir uno nuevo.");

        var turno = new TurnoCaja
        {
            CajaId = caja.Id,
            CodigoSucursal = caja.CodigoSucursal,
            CodigoCaja = caja.Numero,
            UsuarioAperturaId = usuarioId,
            MontoAperturaEfectivo = request.MontoAperturaEfectivo,
            Estado = EstadoTurnoCaja.Abierto
        };

        db.TurnosCaja.Add(turno);
        await db.SaveChangesAsync(ct);

        return turno.Id;
    }

    // Se consulta al elegir una caja: si ya hay un turno abierto (lo haya abierto quien
    // sea), se debe retomar ESE turno en vez de dejar intentar abrir uno nuevo — el turno
    // vive en la base, no en la sesión del navegador de quien lo abrió.
    public async Task<TurnoAbiertoDto?> ObtenerTurnoAbiertoAsync(
        Guid cajaId, IReadOnlyCollection<Guid> sucursalesPermitidas, CancellationToken ct = default)
    {
        var turno = await (
            from t in db.TurnosCaja
            join c in db.Cajas on t.CajaId equals c.Id
            where t.CajaId == cajaId && t.Estado == EstadoTurnoCaja.Abierto && sucursalesPermitidas.Contains(c.SucursalId)
            select t
        ).FirstOrDefaultAsync(ct);

        if (turno is null)
            return null;

        var nombreUsuario = await db.Usuarios.Where(u => u.Id == turno.UsuarioAperturaId).Select(u => u.Nombre).FirstOrDefaultAsync(ct);

        return new TurnoAbiertoDto
        {
            TurnoCajaId = turno.Id,
            NumeroTurno = turno.NumeroTurno,
            FechaHoraApertura = turno.FechaHoraApertura,
            MontoAperturaEfectivo = turno.MontoAperturaEfectivo,
            UsuarioAperturaNombre = nombreUsuario,
            // Un turno no puede cruzar de un día calendario a otro sin cerrarse — se
            // compara en UTC, igual que se guarda FechaHoraApertura.
            EsDeOtroDia = turno.FechaHoraApertura.Date != DateTime.UtcNow.Date
        };
    }

    public async Task<ResumenTurnoDto> ObtenerResumenAsync(
        Guid turnoCajaId, IReadOnlyCollection<Guid> sucursalesPermitidas, CancellationToken ct = default)
    {
        var turno = await ObtenerTurnoDeSucursalPermitidaAsync(turnoCajaId, sucursalesPermitidas, ct);

        var caja = await db.Cajas.Where(c => c.Id == turno.CajaId).Select(c => c.Numero).FirstOrDefaultAsync(ct);

        var usuarioIds = new List<Guid> { turno.UsuarioAperturaId };
        if (turno.UsuarioCierreId is Guid idCierre)
            usuarioIds.Add(idCierre);
        var nombresUsuarios = await db.Usuarios.Where(u => usuarioIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Nombre, ct);

        var ventasPorMetodo = await db.MovimientosCaja
            .Where(m => m.TurnoCajaId == turnoCajaId && m.Tipo == TipoMovimientoCaja.Venta)
            .GroupBy(m => m.MetodoPagoId)
            .Select(g => new { MetodoPagoId = g.Key, Total = g.Sum(m => m.Monto) })
            .ToDictionaryAsync(x => x.MetodoPagoId, x => x.Total, ct);

        // El conteo de billetes/monedas al cerrar incluye el fondo de apertura (nadie lo
        // separa físicamente del cajón) — así que "esperado" en efectivo también debe
        // sumar MontoAperturaEfectivo, o la diferencia saldría siempre desviada por ese
        // monto aunque el cuadre esté perfecto.
        var metodoEfectivoId = await db.MetodosPago.Where(m => m.EsEfectivo).Select(m => m.Id).FirstOrDefaultAsync(ct);

        var denominaciones = await db.DenominacionesCierre
            .Where(d => d.TurnoCajaId == turnoCajaId)
            .ToListAsync(ct);
        var contadoPorMetodo = denominaciones
            .GroupBy(d => d.MetodoPagoId)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.Subtotal));

        var metodoIds = ventasPorMetodo.Keys
            .Concat(contadoPorMetodo.Keys)
            .Concat(metodoEfectivoId != Guid.Empty ? [metodoEfectivoId] : [])
            .Distinct()
            .ToList();
        var nombresMetodos = await db.MetodosPago
            .Where(m => metodoIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Nombre, ct);

        var totales = metodoIds.Select(metodoId =>
        {
            var esperado = ventasPorMetodo.GetValueOrDefault(metodoId) + (metodoId == metodoEfectivoId ? turno.MontoAperturaEfectivo : 0);
            var contado = contadoPorMetodo.GetValueOrDefault(metodoId);
            return new TotalPorFormaPagoDto
            {
                MetodoPagoId = metodoId,
                MetodoPagoNombre = nombresMetodos.GetValueOrDefault(metodoId, "?"),
                Esperado = esperado,
                Contado = turno.Estado == EstadoTurnoCaja.Cerrado ? contado : 0,
                Diferencia = turno.Estado == EstadoTurnoCaja.Cerrado ? contado - esperado : 0
            };
        }).OrderByDescending(t => t.MetodoPagoId == metodoEfectivoId).ThenBy(t => t.MetodoPagoNombre).ToList();

        return new ResumenTurnoDto
        {
            TurnoCajaId = turno.Id,
            NumeroTurno = turno.NumeroTurno,
            Estado = turno.Estado,
            CajaNumero = caja,
            FechaHoraApertura = turno.FechaHoraApertura,
            FechaHoraCierre = turno.FechaHoraCierre,
            MontoAperturaEfectivo = turno.MontoAperturaEfectivo,
            UsuarioAperturaNombre = nombresUsuarios.GetValueOrDefault(turno.UsuarioAperturaId),
            UsuarioCierreNombre = turno.UsuarioCierreId is Guid uc ? nombresUsuarios.GetValueOrDefault(uc) : null,
            Totales = totales,
            Denominaciones = denominaciones
                .Where(d => d.Denominacion is not null)
                .OrderByDescending(d => d.Denominacion)
                .Select(d => new DetalleDenominacionDto
                {
                    MetodoPagoNombre = nombresMetodos.GetValueOrDefault(d.MetodoPagoId, "?"),
                    Denominacion = d.Denominacion,
                    Cantidad = d.Cantidad,
                    Subtotal = d.Subtotal
                }).ToList()
        };
    }

    // Listado de turnos ya cerrados (el "cuadre" queda guardado desde CerrarTurnoAsync,
    // pero hasta ahora no había ninguna pantalla que lo mostrara después del cierre).
    public async Task<ResultadoPaginado<TurnoCerradoResumenDto>> ListarTurnosCerradosAsync(
        Guid sucursalId, Guid? cajaId, DateTime? desde, DateTime? hasta,
        int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 200);

        var query =
            from t in db.TurnosCaja
            join c in db.Cajas on t.CajaId equals c.Id
            where c.SucursalId == sucursalId && t.Estado == EstadoTurnoCaja.Cerrado
            select new { Turno = t, CajaNumero = c.Numero, CajaId = c.Id };

        if (cajaId is not null)
            query = query.Where(x => x.CajaId == cajaId.Value);
        if (desde is not null)
            query = query.Where(x => x.Turno.FechaHoraCierre >= desde.Value);
        if (hasta is not null)
            query = query.Where(x => x.Turno.FechaHoraCierre <= hasta.Value);

        var total = await query.CountAsync(ct);

        var turnos = await query
            .OrderByDescending(x => x.Turno.FechaHoraCierre)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(x => new { x.Turno, x.CajaNumero })
            .ToListAsync(ct);

        var turnoIds = turnos.Select(t => t.Turno.Id).ToList();

        var metodoEfectivoId = await db.MetodosPago.Where(m => m.EsEfectivo).Select(m => m.Id).FirstOrDefaultAsync(ct);

        var ventasPorTurno = await db.MovimientosCaja
            .Where(m => turnoIds.Contains(m.TurnoCajaId) && m.Tipo == TipoMovimientoCaja.Venta)
            .GroupBy(m => m.TurnoCajaId)
            .Select(g => new { TurnoCajaId = g.Key, Total = g.Sum(m => m.Monto) })
            .ToDictionaryAsync(x => x.TurnoCajaId, x => x.Total, ct);

        var esperadoPorTurnoYMetodo = await db.MovimientosCaja
            .Where(m => turnoIds.Contains(m.TurnoCajaId) && m.Tipo == TipoMovimientoCaja.Venta)
            .GroupBy(m => new { m.TurnoCajaId, m.MetodoPagoId })
            .Select(g => new { g.Key.TurnoCajaId, g.Key.MetodoPagoId, Total = g.Sum(m => m.Monto) })
            .ToListAsync(ct);

        var contadoPorTurno = await db.DenominacionesCierre
            .Where(d => turnoIds.Contains(d.TurnoCajaId))
            .GroupBy(d => d.TurnoCajaId)
            .Select(g => new { TurnoCajaId = g.Key, Total = g.Sum(d => d.Subtotal) })
            .ToDictionaryAsync(x => x.TurnoCajaId, x => x.Total, ct);

        var usuarioIds = turnos.Select(t => t.Turno.UsuarioAperturaId)
            .Concat(turnos.Where(t => t.Turno.UsuarioCierreId.HasValue).Select(t => t.Turno.UsuarioCierreId!.Value))
            .Distinct().ToList();
        var nombresUsuarios = await db.Usuarios.Where(u => usuarioIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Nombre, ct);

        var items = turnos.Select(t =>
        {
            var esperadoTotal = esperadoPorTurnoYMetodo.Where(e => e.TurnoCajaId == t.Turno.Id).Sum(e => e.Total)
                + t.Turno.MontoAperturaEfectivo;
            var contadoTotal = contadoPorTurno.GetValueOrDefault(t.Turno.Id);

            return new TurnoCerradoResumenDto
            {
                TurnoCajaId = t.Turno.Id,
                NumeroTurno = t.Turno.NumeroTurno,
                CajaNumero = t.CajaNumero,
                FechaHoraApertura = t.Turno.FechaHoraApertura,
                FechaHoraCierre = t.Turno.FechaHoraCierre,
                UsuarioAperturaNombre = nombresUsuarios.GetValueOrDefault(t.Turno.UsuarioAperturaId),
                UsuarioCierreNombre = t.Turno.UsuarioCierreId is Guid idCierre ? nombresUsuarios.GetValueOrDefault(idCierre) : null,
                TotalVendido = ventasPorTurno.GetValueOrDefault(t.Turno.Id),
                DiferenciaTotal = contadoTotal - esperadoTotal
            };
        }).ToList();

        return new ResultadoPaginado<TurnoCerradoResumenDto>
        {
            Items = items,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = total
        };
    }

    public async Task CerrarTurnoAsync(
        Guid usuarioId, IReadOnlyCollection<Guid> sucursalesPermitidas, CerrarTurnoRequestDto request, CancellationToken ct = default)
    {
        var turno = await ObtenerTurnoDeSucursalPermitidaAsync(request.TurnoCajaId, sucursalesPermitidas, ct);

        if (turno.Estado != EstadoTurnoCaja.Abierto)
            throw new InvalidOperationException("El turno ya está cerrado.");

        foreach (var d in request.Denominaciones)
        {
            db.DenominacionesCierre.Add(new DenominacionCierre
            {
                TurnoCajaId = turno.Id,
                CodigoSucursal = turno.CodigoSucursal,
                CodigoCaja = turno.CodigoCaja,
                MetodoPagoId = d.MetodoPagoId,
                Denominacion = d.Denominacion,
                Cantidad = d.Cantidad,
                Subtotal = (d.Denominacion ?? 1) * d.Cantidad
            });
        }

        turno.Estado = EstadoTurnoCaja.Cerrado;
        turno.UsuarioCierreId = usuarioId;
        turno.FechaHoraCierre = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var caja = await db.Cajas.FirstOrDefaultAsync(c => c.Id == turno.CajaId, ct);
        await auditoria.RegistrarAsync(caja?.SucursalId, usuarioId, "CierreCaja", "TurnoCaja", turno.Id, ct: ct);
    }

    // Evita IDOR: sin esto, cualquier usuario autenticado podía leer/cerrar el turno
    // de una caja de OTRA sucursal con solo conocer su GUID.
    private async Task<TurnoCaja> ObtenerTurnoDeSucursalPermitidaAsync(
        Guid turnoCajaId, IReadOnlyCollection<Guid> sucursalesPermitidas, CancellationToken ct)
    {
        var turno = await (
            from t in db.TurnosCaja
            join c in db.Cajas on t.CajaId equals c.Id
            where t.Id == turnoCajaId && sucursalesPermitidas.Contains(c.SucursalId)
            select t
        ).FirstOrDefaultAsync(ct);

        return turno ?? throw new InvalidOperationException("El turno no existe.");
    }
}
