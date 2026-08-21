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
            .Select(c => new CajaResumenDto { Id = c.Id, Numero = c.Numero, Activa = c.Activa })
            .ToListAsync(ct);

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
            UsuarioAperturaId = usuarioId,
            MontoAperturaEfectivo = request.MontoAperturaEfectivo,
            Estado = EstadoTurnoCaja.Abierto
        };

        db.TurnosCaja.Add(turno);
        await db.SaveChangesAsync(ct);

        return turno.Id;
    }

    public async Task<ResumenTurnoDto> ObtenerResumenAsync(
        Guid turnoCajaId, IReadOnlyCollection<Guid> sucursalesPermitidas, CancellationToken ct = default)
    {
        var turno = await ObtenerTurnoDeSucursalPermitidaAsync(turnoCajaId, sucursalesPermitidas, ct);

        var totalesEsperados = await db.MovimientosCaja
            .Where(m => m.TurnoCajaId == turnoCajaId && m.Tipo == TipoMovimientoCaja.Venta)
            .GroupBy(m => m.FormaPago)
            .Select(g => new { FormaPago = g.Key, Total = g.Sum(m => m.Monto) })
            .ToListAsync(ct);

        return new ResumenTurnoDto
        {
            TurnoCajaId = turno.Id,
            Estado = turno.Estado,
            FechaHoraApertura = turno.FechaHoraApertura,
            FechaHoraCierre = turno.FechaHoraCierre,
            MontoAperturaEfectivo = turno.MontoAperturaEfectivo,
            Totales = totalesEsperados.Select(t => new TotalPorFormaPagoDto
            {
                FormaPago = t.FormaPago,
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
                FormaPago = d.FormaPago,
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
