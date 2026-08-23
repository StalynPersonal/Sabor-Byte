using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Caja.Dtos;
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

        var totalesEsperados = await db.MovimientosCaja
            .Where(m => m.TurnoCajaId == turnoCajaId && m.Tipo == TipoMovimientoCaja.Venta)
            .GroupBy(m => m.MetodoPagoId)
            .Select(g => new { MetodoPagoId = g.Key, Total = g.Sum(m => m.Monto) })
            .ToListAsync(ct);

        var nombresMetodos = await db.MetodosPago
            .Where(m => totalesEsperados.Select(t => t.MetodoPagoId).Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Nombre, ct);

        return new ResumenTurnoDto
        {
            TurnoCajaId = turno.Id,
            Estado = turno.Estado,
            FechaHoraApertura = turno.FechaHoraApertura,
            FechaHoraCierre = turno.FechaHoraCierre,
            MontoAperturaEfectivo = turno.MontoAperturaEfectivo,
            Totales = totalesEsperados.Select(t => new TotalPorFormaPagoDto
            {
                MetodoPagoId = t.MetodoPagoId,
                MetodoPagoNombre = nombresMetodos.GetValueOrDefault(t.MetodoPagoId, ""),
                Esperado = t.Total
            }).ToList()
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
