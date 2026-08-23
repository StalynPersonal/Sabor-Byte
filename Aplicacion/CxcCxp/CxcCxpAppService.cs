using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.CxcCxp.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.CxcCxp;

namespace SaborByte.Aplicacion.CxcCxp;

public class CxcCxpAppService(IAppDbContext db)
{
    // --- Proveedores ---

    public async Task<List<ProveedorDto>> ListarProveedoresAsync(Guid sucursalId, bool incluirInactivos = false, CancellationToken ct = default) =>
        await db.Proveedores
            .Where(p => p.SucursalId == sucursalId && (incluirInactivos || p.Activo))
            .OrderBy(p => p.NombreORazonSocial)
            .Select(p => new ProveedorDto { Id = p.Id, NombreORazonSocial = p.NombreORazonSocial, Rnc = p.Rnc, Telefono = p.Telefono, Activo = p.Activo })
            .ToListAsync(ct);

    public async Task<Guid> CrearProveedorAsync(Guid sucursalId, Guid usuarioId, GuardarProveedorRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NombreORazonSocial))
            throw new InvalidOperationException("El nombre del proveedor es obligatorio.");

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

    public async Task ActualizarProveedorAsync(Guid sucursalId, Guid proveedorId, GuardarProveedorRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NombreORazonSocial))
            throw new InvalidOperationException("El nombre del proveedor es obligatorio.");

        var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Id == proveedorId && p.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El proveedor no existe.");

        proveedor.NombreORazonSocial = request.NombreORazonSocial;
        proveedor.Rnc = request.Rnc;
        proveedor.Telefono = request.Telefono;
        proveedor.Activo = request.Activo;

        await db.SaveChangesAsync(ct);
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
        Guid sucursalId, int pagina, int tamanoPagina, bool incluirPagadas = false, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.CuentasPorCobrar.Where(c => c.SucursalId == sucursalId);
        if (!incluirPagadas)
            query = query.Where(c => c.Estado != EstadoCuenta.Pagada);

        var total = await query.CountAsync(ct);

        var items = await (
                from c in query
                join cl in db.Clientes on c.ClienteId equals cl.Id
                orderby c.FechaVencimiento
                select new CuentaPorCobrarDto
                {
                    Id = c.Id,
                    ClienteId = c.ClienteId,
                    ClienteNombre = cl.NombreORazonSocial,
                    MontoOriginal = c.MontoOriginal,
                    SaldoPendiente = c.SaldoPendiente,
                    FechaVencimiento = c.FechaVencimiento,
                    Estado = c.Estado
                })
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new ResultadoPaginado<CuentaPorCobrarDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = total };
    }

    public async Task<List<PagoCuentaDto>> ObtenerPagosCxCAsync(Guid sucursalId, Guid cuentaId, CancellationToken ct = default)
    {
        var existe = await db.CuentasPorCobrar.AnyAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct);
        if (!existe)
            throw new InvalidOperationException("La cuenta por cobrar no existe.");

        return await (
                from p in db.PagosCxC
                join m in db.MetodosPago on p.MetodoPagoId equals m.Id
                where p.CuentaPorCobrarId == cuentaId
                orderby p.FechaPago descending
                select new PagoCuentaDto { Id = p.Id, FechaPago = p.FechaPago, Monto = p.Monto, MetodoPagoNombre = m.Nombre }
            )
            .ToListAsync(ct);
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
        Guid sucursalId, int pagina, int tamanoPagina, bool incluirPagadas = false, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.CuentasPorPagar.Where(c => c.SucursalId == sucursalId);
        if (!incluirPagadas)
            query = query.Where(c => c.Estado != EstadoCuenta.Pagada);

        var total = await query.CountAsync(ct);

        var items = await (
                from c in query
                join pr in db.Proveedores on c.ProveedorId equals pr.Id
                orderby c.FechaVencimiento
                select new CuentaPorPagarDto
                {
                    Id = c.Id,
                    ProveedorId = c.ProveedorId,
                    ProveedorNombre = pr.NombreORazonSocial,
                    DocumentoReferencia = c.DocumentoReferencia,
                    MontoOriginal = c.MontoOriginal,
                    SaldoPendiente = c.SaldoPendiente,
                    FechaVencimiento = c.FechaVencimiento,
                    Estado = c.Estado
                })
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new ResultadoPaginado<CuentaPorPagarDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = total };
    }

    public async Task<List<PagoCuentaDto>> ObtenerPagosCxPAsync(Guid sucursalId, Guid cuentaId, CancellationToken ct = default)
    {
        var existe = await db.CuentasPorPagar.AnyAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct);
        if (!existe)
            throw new InvalidOperationException("La cuenta por pagar no existe.");

        return await (
                from p in db.PagosCxP
                join m in db.MetodosPago on p.MetodoPagoId equals m.Id
                where p.CuentaPorPagarId == cuentaId
                orderby p.FechaPago descending
                select new PagoCuentaDto { Id = p.Id, FechaPago = p.FechaPago, Monto = p.Monto, MetodoPagoNombre = m.Nombre }
            )
            .ToListAsync(ct);
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
