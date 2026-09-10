using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Gastos.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Caja;
using SaborByte.Dominio.Gastos;

namespace SaborByte.Aplicacion.Gastos;

public class GastoAppService(IAppDbContext db)
{
    // --- Categorías (catálogo global) ---

    public async Task<List<CategoriaGastoDto>> ListarCategoriasAsync(bool incluirInactivas, CancellationToken ct = default) =>
        await db.CategoriasGasto
            .Where(c => incluirInactivas || c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaGastoDto { Id = c.Id, Nombre = c.Nombre, Activo = c.Activo })
            .ToListAsync(ct);

    public async Task<Guid> CrearCategoriaAsync(GuardarCategoriaGastoRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre de la categoría es obligatorio.");

        var yaExiste = await db.CategoriasGasto.AnyAsync(c => c.Nombre == request.Nombre, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe una categoría de gasto llamada '{request.Nombre}'.");

        var categoria = new CategoriaGasto { Nombre = request.Nombre, Activo = request.Activo };
        db.CategoriasGasto.Add(categoria);
        await db.SaveChangesAsync(ct);
        return categoria.Id;
    }

    public async Task ActualizarCategoriaAsync(Guid categoriaGastoId, GuardarCategoriaGastoRequestDto request, CancellationToken ct = default)
    {
        var categoria = await db.CategoriasGasto.FirstOrDefaultAsync(c => c.Id == categoriaGastoId, ct)
            ?? throw new InvalidOperationException("La categoría no existe.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new InvalidOperationException("El nombre de la categoría es obligatorio.");

        var yaExiste = await db.CategoriasGasto.AnyAsync(c => c.Id != categoriaGastoId && c.Nombre == request.Nombre, ct);
        if (yaExiste)
            throw new InvalidOperationException($"Ya existe otra categoría de gasto llamada '{request.Nombre}'.");

        categoria.Nombre = request.Nombre;
        categoria.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
    }

    // --- Gastos ---

    public async Task<List<GastoDto>> BuscarAsync(
        Guid sucursalId, DateTime desde, DateTime hasta, Guid? categoriaGastoId, string? texto, CancellationToken ct = default)
    {
        var query = db.Gastos.Where(g =>
            g.SucursalId == sucursalId && g.FechaGasto >= desde && g.FechaGasto <= hasta);

        if (categoriaGastoId is not null)
            query = query.Where(g => g.CategoriaGastoId == categoriaGastoId.Value);

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(g =>
                EF.Functions.Like(g.Descripcion, $"%{texto}%") ||
                EF.Functions.Like(g.CategoriaGasto!.Nombre, $"%{texto}%"));

        return await (
                from g in query
                join m in db.MetodosPago on g.MetodoPagoId equals m.Id
                join u in db.Usuarios on g.CreadoPorUsuarioId equals u.Id
                join ua in db.Usuarios on g.AnuladoPorUsuarioId equals ua.Id into anuladores
                from ua in anuladores.DefaultIfEmpty()
                orderby g.FechaGasto descending
                select new GastoDto
                {
                    Id = g.Id,
                    FechaGasto = g.FechaGasto,
                    CategoriaGastoId = g.CategoriaGastoId,
                    CategoriaGastoNombre = g.CategoriaGasto!.Nombre,
                    Descripcion = g.Descripcion,
                    Monto = g.Monto,
                    EsGastoDelNegocio = g.EsGastoDelNegocio,
                    MetodoPagoNombre = m.Nombre,
                    AfectoCaja = g.MovimientoCajaId != null,
                    RegistradoPorNombre = u.Nombre,
                    Anulado = g.Anulado,
                    FechaAnulacion = g.FechaAnulacion,
                    AnuladoPorNombre = ua != null ? ua.Nombre : null,
                    MotivoAnulacion = g.MotivoAnulacion
                }
            )
            .ToListAsync(ct);
    }

    public async Task<ResumenGastosDto> ObtenerResumenAsync(
        Guid sucursalId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var gastos = await db.Gastos
            .Where(g => g.SucursalId == sucursalId && g.FechaGasto >= desde && g.FechaGasto <= hasta
                && !g.Anulado && g.EsGastoDelNegocio)
            .Select(g => new { g.Monto, CategoriaNombre = g.CategoriaGasto!.Nombre })
            .ToListAsync(ct);

        return new ResumenGastosDto
        {
            TotalPeriodo = gastos.Sum(g => g.Monto),
            PorCategoria = gastos
                .GroupBy(g => g.CategoriaNombre)
                .Select(gr => new GastoPorCategoriaDto { CategoriaGastoNombre = gr.Key, Cantidad = gr.Count(), Total = gr.Sum(g => g.Monto) })
                .OrderByDescending(g => g.Total)
                .ToList()
        };
    }

    public async Task<Guid> RegistrarAsync(Guid sucursalId, Guid usuarioId, RegistrarGastoRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Descripcion))
            throw new InvalidOperationException("La descripción del gasto es obligatoria.");

        if (request.Monto <= 0)
            throw new InvalidOperationException("El monto debe ser mayor a cero.");

        var categoria = await db.CategoriasGasto.FirstOrDefaultAsync(c => c.Id == request.CategoriaGastoId, ct)
            ?? throw new InvalidOperationException("La categoría no existe.");

        var metodoPago = await db.MetodosPago.FirstOrDefaultAsync(m => m.Id == request.MetodoPagoId, ct)
            ?? throw new InvalidOperationException("El método de pago no existe.");

        var gasto = new Gasto
        {
            SucursalId = sucursalId,
            FechaGasto = request.FechaGasto,
            CategoriaGastoId = categoria.Id,
            Descripcion = request.Descripcion,
            Monto = request.Monto,
            EsGastoDelNegocio = request.EsGastoDelNegocio,
            MetodoPagoId = metodoPago.Id,
            CreadoPorUsuarioId = usuarioId
        };

        // Solo si se pagó en efectivo y se indicó un turno abierto: se resta del efectivo
        // esperado de esa caja (MovimientoCaja tipo Salida, monto negativo) para que el
        // cuadre no muestre una diferencia sin explicar — ver CajaAppService.ObtenerResumenAsync.
        if (metodoPago.EsEfectivo && request.TurnoCajaId is Guid turnoCajaId)
        {
            var turno = await db.TurnosCaja.FirstOrDefaultAsync(t => t.Id == turnoCajaId && t.Estado == EstadoTurnoCaja.Abierto, ct);
            if (turno is not null)
            {
                var movimiento = new MovimientoCaja
                {
                    TurnoCajaId = turno.Id,
                    Tipo = TipoMovimientoCaja.Salida,
                    MetodoPagoId = metodoPago.Id,
                    Monto = -request.Monto,
                    Descripcion = $"Gasto: {request.Descripcion}"
                };
                db.MovimientosCaja.Add(movimiento);
                gasto.TurnoCajaId = turno.Id;
                gasto.MovimientoCajaId = movimiento.Id;
            }
        }

        db.Gastos.Add(gasto);
        await db.SaveChangesAsync(ct);
        return gasto.Id;
    }

    public async Task AnularAsync(Guid sucursalId, Guid gastoId, Guid usuarioId, AnularGastoRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new InvalidOperationException("El motivo de la anulación es obligatorio.");

        var gasto = await db.Gastos.FirstOrDefaultAsync(g => g.Id == gastoId && g.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El gasto no existe.");

        if (gasto.Anulado)
            throw new InvalidOperationException("Este gasto ya fue anulado.");

        gasto.Anulado = true;
        gasto.FechaAnulacion = DateTime.UtcNow;
        gasto.AnuladoPorUsuarioId = usuarioId;
        gasto.MotivoAnulacion = request.Motivo;

        // Revierte el efecto en la caja: se borra el movimiento de salida, el efectivo
        // esperado de esa caja vuelve a ser como si el gasto nunca hubiera pasado.
        if (gasto.MovimientoCajaId is Guid movimientoCajaId)
        {
            var movimiento = await db.MovimientosCaja.FirstOrDefaultAsync(m => m.Id == movimientoCajaId, ct);
            if (movimiento is not null)
                db.MovimientosCaja.Remove(movimiento);
        }

        await db.SaveChangesAsync(ct);
    }
}
