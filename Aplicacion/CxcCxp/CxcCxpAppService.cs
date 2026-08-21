using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.CxcCxp.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.CxcCxp;

namespace SaborByte.Aplicacion.CxcCxp;

public class CxcCxpAppService(IAppDbContext db)
{
    // --- Cuentas por Cobrar ---

    public async Task<Guid> CrearCuentaPorCobrarAsync(Guid sucursalId, CrearCuentaPorCobrarRequestDto request, CancellationToken ct = default)
    {
        var cuenta = new CuentaPorCobrar
        {
            SucursalId = sucursalId,
            ClienteId = request.ClienteId,
            FacturaId = request.FacturaId,
            MontoOriginal = request.MontoOriginal,
            SaldoPendiente = request.MontoOriginal,
            FechaVencimiento = request.FechaVencimiento
        };

        db.CuentasPorCobrar.Add(cuenta);
        await db.SaveChangesAsync(ct);
        return cuenta.Id;
    }

    public async Task<List<CuentaPorCobrarDto>> ListarPorCobrarAsync(Guid sucursalId, CancellationToken ct = default) =>
        await db.CuentasPorCobrar
            .Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada)
            .OrderBy(c => c.FechaVencimiento)
            .Select(c => new CuentaPorCobrarDto
            {
                Id = c.Id,
                ClienteId = c.ClienteId,
                MontoOriginal = c.MontoOriginal,
                SaldoPendiente = c.SaldoPendiente,
                FechaVencimiento = c.FechaVencimiento,
                Estado = c.Estado
            })
            .ToListAsync(ct);

    public async Task RegistrarPagoCxCAsync(Guid sucursalId, Guid cuentaId, RegistrarPagoRequestDto request, CancellationToken ct = default)
    {
        var cuenta = await db.CuentasPorCobrar.FirstOrDefaultAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cuenta por cobrar no existe.");

        if (request.Monto <= 0 || request.Monto > cuenta.SaldoPendiente)
            throw new InvalidOperationException("El monto del pago no es válido para el saldo pendiente.");

        db.PagosCxC.Add(new PagoCxC { CuentaPorCobrarId = cuenta.Id, Monto = request.Monto, FormaPago = request.FormaPago });

        cuenta.SaldoPendiente -= request.Monto;
        cuenta.Estado = cuenta.SaldoPendiente == 0 ? EstadoCuenta.Pagada : EstadoCuenta.PagadaParcial;

        await db.SaveChangesAsync(ct);
    }

    // --- Cuentas por Pagar ---

    public async Task<Guid> CrearCuentaPorPagarAsync(Guid sucursalId, CrearCuentaPorPagarRequestDto request, CancellationToken ct = default)
    {
        var cuenta = new CuentaPorPagar
        {
            SucursalId = sucursalId,
            ProveedorId = request.ProveedorId,
            DocumentoReferencia = request.DocumentoReferencia,
            MontoOriginal = request.MontoOriginal,
            SaldoPendiente = request.MontoOriginal,
            FechaVencimiento = request.FechaVencimiento
        };

        db.CuentasPorPagar.Add(cuenta);
        await db.SaveChangesAsync(ct);
        return cuenta.Id;
    }

    public async Task<List<CuentaPorPagarDto>> ListarPorPagarAsync(Guid sucursalId, CancellationToken ct = default) =>
        await db.CuentasPorPagar
            .Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada)
            .OrderBy(c => c.FechaVencimiento)
            .Select(c => new CuentaPorPagarDto
            {
                Id = c.Id,
                ProveedorId = c.ProveedorId,
                DocumentoReferencia = c.DocumentoReferencia,
                MontoOriginal = c.MontoOriginal,
                SaldoPendiente = c.SaldoPendiente,
                FechaVencimiento = c.FechaVencimiento,
                Estado = c.Estado
            })
            .ToListAsync(ct);

    public async Task RegistrarPagoCxPAsync(Guid sucursalId, Guid cuentaId, RegistrarPagoRequestDto request, CancellationToken ct = default)
    {
        var cuenta = await db.CuentasPorPagar.FirstOrDefaultAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cuenta por pagar no existe.");

        if (request.Monto <= 0 || request.Monto > cuenta.SaldoPendiente)
            throw new InvalidOperationException("El monto del pago no es válido para el saldo pendiente.");

        db.PagosCxP.Add(new PagoCxP { CuentaPorPagarId = cuenta.Id, Monto = request.Monto, FormaPago = request.FormaPago });

        cuenta.SaldoPendiente -= request.Monto;
        cuenta.Estado = cuenta.SaldoPendiente == 0 ? EstadoCuenta.Pagada : EstadoCuenta.PagadaParcial;

        await db.SaveChangesAsync(ct);
    }
}
