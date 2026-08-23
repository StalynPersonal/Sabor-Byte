using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.CxcCxp.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.CxcCxp;

namespace SaborByte.Aplicacion.CxcCxp;

public class CxcCxpAppService(IAppDbContext db)
{
    // --- Proveedores (necesarios para dar de alta una Cuenta por Pagar) ---

    public async Task<List<ProveedorDto>> ListarProveedoresAsync(Guid sucursalId, CancellationToken ct = default) =>
        await db.Proveedores
            .Where(p => p.SucursalId == sucursalId && p.Activo)
            .OrderBy(p => p.NombreORazonSocial)
            .Select(p => new ProveedorDto { Id = p.Id, NombreORazonSocial = p.NombreORazonSocial, Rnc = p.Rnc, Telefono = p.Telefono, Activo = p.Activo })
            .ToListAsync(ct);

    public async Task<Guid> CrearProveedorAsync(Guid sucursalId, Guid usuarioId, GuardarProveedorRequestDto request, CancellationToken ct = default)
    {
        var proveedor = new Proveedor
        {
            SucursalId = sucursalId,
            NombreORazonSocial = request.NombreORazonSocial,
            Rnc = request.Rnc,
            Telefono = request.Telefono,
            CreadoPorUsuarioId = usuarioId
        };

        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync(ct);
        return proveedor.Id;
    }

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

    public async Task<ResultadoPaginado<CuentaPorCobrarDto>> ListarPorCobrarAsync(
        Guid sucursalId, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.CuentasPorCobrar.Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.FechaVencimiento)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
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

        return new ResultadoPaginado<CuentaPorCobrarDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = total };
    }

    public async Task RegistrarPagoCxCAsync(Guid sucursalId, Guid cuentaId, Guid usuarioId, RegistrarPagoRequestDto request, CancellationToken ct = default)
    {
        var cuenta = await db.CuentasPorCobrar.FirstOrDefaultAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cuenta por cobrar no existe.");

        if (request.Monto <= 0 || request.Monto > cuenta.SaldoPendiente)
            throw new InvalidOperationException("El monto del pago no es válido para el saldo pendiente.");

        db.PagosCxC.Add(new PagoCxC { CuentaPorCobrarId = cuenta.Id, Monto = request.Monto, MetodoPagoId = request.MetodoPagoId, CreadoPorUsuarioId = usuarioId });

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

    public async Task<ResultadoPaginado<CuentaPorPagarDto>> ListarPorPagarAsync(
        Guid sucursalId, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.CuentasPorPagar.Where(c => c.SucursalId == sucursalId && c.Estado != EstadoCuenta.Pagada);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.FechaVencimiento)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
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

        return new ResultadoPaginado<CuentaPorPagarDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = total };
    }

    public async Task RegistrarPagoCxPAsync(Guid sucursalId, Guid cuentaId, Guid usuarioId, RegistrarPagoRequestDto request, CancellationToken ct = default)
    {
        var cuenta = await db.CuentasPorPagar.FirstOrDefaultAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cuenta por pagar no existe.");

        if (request.Monto <= 0 || request.Monto > cuenta.SaldoPendiente)
            throw new InvalidOperationException("El monto del pago no es válido para el saldo pendiente.");

        db.PagosCxP.Add(new PagoCxP { CuentaPorPagarId = cuenta.Id, Monto = request.Monto, MetodoPagoId = request.MetodoPagoId, CreadoPorUsuarioId = usuarioId });

        cuenta.SaldoPendiente -= request.Monto;
        cuenta.Estado = cuenta.SaldoPendiente == 0 ? EstadoCuenta.Pagada : EstadoCuenta.PagadaParcial;

        await db.SaveChangesAsync(ct);
    }
}
