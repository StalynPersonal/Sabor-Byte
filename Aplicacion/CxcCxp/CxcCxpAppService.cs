using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Comun;
using SaborByte.Aplicacion.CxcCxp.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.CxcCxp;

namespace SaborByte.Aplicacion.CxcCxp;

public class CxcCxpAppService(IAppDbContext db, IAuditoriaService auditoria)
{
    // --- Proveedores ---

    public async Task<List<ProveedorDto>> ListarProveedoresAsync(Guid sucursalId, bool incluirInactivos = false, CancellationToken ct = default) =>
        await db.Proveedores
            .Where(p => p.SucursalId == sucursalId && (incluirInactivos || p.Activo))
            .OrderBy(p => p.NombreORazonSocial)
            .Select(p => new ProveedorDto { Id = p.Id, NombreORazonSocial = p.NombreORazonSocial, Rnc = p.Rnc, Telefono = p.Telefono, Activo = p.Activo })
            .ToListAsync(ct);

    public async Task<ResultadoPaginado<ProveedorDto>> ListarProveedoresPaginadoAsync(
        Guid sucursalId, int pagina, int tamanoPagina, bool incluirInactivos = false, string? texto = null, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.Proveedores.Where(p => p.SucursalId == sucursalId && (incluirInactivos || p.Activo));

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(p =>
                EF.Functions.Like(p.NombreORazonSocial, $"%{texto}%") ||
                (p.Rnc != null && EF.Functions.Like(p.Rnc, $"%{texto}%")));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.NombreORazonSocial)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(p => new ProveedorDto { Id = p.Id, NombreORazonSocial = p.NombreORazonSocial, Rnc = p.Rnc, Telefono = p.Telefono, Activo = p.Activo })
            .ToListAsync(ct);

        return new ResultadoPaginado<ProveedorDto> { Items = items, Pagina = pagina, TamanoPagina = tamanoPagina, TotalRegistros = total };
    }

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

    public async Task<Guid> CrearCuentaPorCobrarAsync(Guid sucursalId, Guid usuarioId, CrearCuentaPorCobrarRequestDto request, CancellationToken ct = default)
    {
        var cuenta = new CuentaPorCobrar
        {
            SucursalId = sucursalId,
            ClienteId = request.ClienteId,
            FacturaId = request.FacturaId,
            MontoOriginal = request.MontoOriginal,
            SaldoPendiente = request.MontoOriginal,
            FechaVencimiento = request.FechaVencimiento,
            CreadoPorUsuarioId = usuarioId
        };

        db.CuentasPorCobrar.Add(cuenta);
        await db.SaveChangesAsync(ct);
        return cuenta.Id;
    }

    public async Task<ResultadoPaginado<CuentaPorCobrarDto>> ListarPorCobrarAsync(
        Guid sucursalId, int pagina, int tamanoPagina, bool incluirPagadas = false, string? texto = null, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.CuentasPorCobrar.Where(c => c.SucursalId == sucursalId);
        if (!incluirPagadas)
            query = query.Where(c => c.Estado != EstadoCuenta.Pagada);

        var consulta =
            from c in query
            join cl in db.Clientes on c.ClienteId equals cl.Id
            join u in db.Usuarios on c.CreadoPorUsuarioId equals u.Id into usuarios
            from u in usuarios.DefaultIfEmpty()
            select new { Cuenta = c, Cliente = cl, Usuario = u };

        if (!string.IsNullOrWhiteSpace(texto))
            consulta = consulta.Where(x =>
                EF.Functions.Like(x.Cliente.NombreORazonSocial, $"%{texto}%") ||
                (x.Cliente.RncOCedula != null && EF.Functions.Like(x.Cliente.RncOCedula, $"%{texto}%")));

        var total = await consulta.CountAsync(ct);

        var items = await (
                from x in consulta
                let c = x.Cuenta
                let cl = x.Cliente
                orderby c.FechaVencimiento
                select new CuentaPorCobrarDto
                {
                    Id = c.Id,
                    ClienteId = c.ClienteId,
                    ClienteNombre = cl.NombreORazonSocial,
                    MontoOriginal = c.MontoOriginal,
                    SaldoPendiente = c.SaldoPendiente,
                    FechaVencimiento = c.FechaVencimiento,
                    Estado = c.Estado,
                    RegistradaPorNombre = x.Usuario != null ? x.Usuario.Nombre : "—"
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
                join u in db.Usuarios on p.CreadoPorUsuarioId equals u.Id
                join ua in db.Usuarios on p.AnuladoPorUsuarioId equals ua.Id into anuladores
                from ua in anuladores.DefaultIfEmpty()
                where p.CuentaPorCobrarId == cuentaId
                orderby p.FechaPago descending
                select new PagoCuentaDto
                {
                    Id = p.Id,
                    FechaPago = p.FechaPago,
                    Monto = p.Monto,
                    MetodoPagoNombre = m.Nombre,
                    NumeroComprobante = p.NumeroComprobante,
                    RegistradoPorNombre = u.Nombre,
                    Anulado = p.Anulado,
                    FechaAnulacion = p.FechaAnulacion,
                    AnuladoPorNombre = ua != null ? ua.Nombre : null,
                    MotivoAnulacion = p.MotivoAnulacion
                }
            )
            .ToListAsync(ct);
    }

    public async Task RegistrarPagoCxCAsync(Guid sucursalId, Guid cuentaId, Guid usuarioId, RegistrarPagoRequestDto request, CancellationToken ct = default)
    {
        var cuenta = await db.CuentasPorCobrar.FirstOrDefaultAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cuenta por cobrar no existe.");

        if (request.Monto <= 0 || request.Monto > cuenta.SaldoPendiente)
            throw new InvalidOperationException("El monto del pago no es válido para el saldo pendiente.");

        var metodoPago = await db.MetodosPago.FirstOrDefaultAsync(m => m.Id == request.MetodoPagoId, ct)
            ?? throw new InvalidOperationException("El método de pago no existe.");

        if (metodoPago.RequiereComprobante && string.IsNullOrWhiteSpace(request.NumeroComprobante))
            throw new InvalidOperationException($"El método de pago \"{metodoPago.Nombre}\" requiere número de comprobante.");

        db.PagosCxC.Add(new PagoCxC
        {
            CuentaPorCobrarId = cuenta.Id,
            Monto = request.Monto,
            MetodoPagoId = request.MetodoPagoId,
            CreadoPorUsuarioId = usuarioId,
            NumeroComprobante = metodoPago.RequiereComprobante ? request.NumeroComprobante : null
        });

        cuenta.SaldoPendiente -= request.Monto;
        cuenta.Estado = cuenta.SaldoPendiente == 0 ? EstadoCuenta.Pagada : EstadoCuenta.PagadaParcial;

        await db.SaveChangesAsync(ct);
    }

    // No se borra el pago: se marca Anulado (con motivo y quién/cuándo) y se devuelve el
    // monto al saldo pendiente de la cuenta, preservando el historial para auditoría.
    public async Task AnularPagoCxCAsync(Guid sucursalId, Guid cuentaId, Guid pagoId, Guid usuarioId, AnularPagoRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new InvalidOperationException("El motivo de la anulación es obligatorio.");

        var cuenta = await db.CuentasPorCobrar.FirstOrDefaultAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cuenta por cobrar no existe.");

        var pago = await db.PagosCxC.FirstOrDefaultAsync(p => p.Id == pagoId && p.CuentaPorCobrarId == cuentaId, ct)
            ?? throw new InvalidOperationException("El pago no existe.");

        if (pago.Anulado)
            throw new InvalidOperationException("Este pago ya fue anulado.");

        pago.Anulado = true;
        pago.FechaAnulacion = DateTime.UtcNow;
        pago.AnuladoPorUsuarioId = usuarioId;
        pago.MotivoAnulacion = request.Motivo;

        cuenta.SaldoPendiente += pago.Monto;
        cuenta.Estado = cuenta.SaldoPendiente == cuenta.MontoOriginal ? EstadoCuenta.Pendiente : EstadoCuenta.PagadaParcial;

        await db.SaveChangesAsync(ct);
        await auditoria.RegistrarAsync(sucursalId, usuarioId, "AnulacionPago", "PagoCxC", pago.Id, request.Motivo, ct);
    }

    // --- Cuentas por Pagar ---

    public async Task<Guid> CrearCuentaPorPagarAsync(Guid sucursalId, Guid usuarioId, CrearCuentaPorPagarRequestDto request, CancellationToken ct = default)
    {
        var cuenta = new CuentaPorPagar
        {
            SucursalId = sucursalId,
            ProveedorId = request.ProveedorId,
            DocumentoReferencia = request.DocumentoReferencia,
            MontoOriginal = request.MontoOriginal,
            SaldoPendiente = request.MontoOriginal,
            FechaVencimiento = request.FechaVencimiento,
            CreadoPorUsuarioId = usuarioId
        };

        db.CuentasPorPagar.Add(cuenta);
        await db.SaveChangesAsync(ct);
        return cuenta.Id;
    }

    public async Task<ResultadoPaginado<CuentaPorPagarDto>> ListarPorPagarAsync(
        Guid sucursalId, int pagina, int tamanoPagina, bool incluirPagadas = false, string? texto = null, CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanoPagina = tamanoPagina is < 1 or > 200 ? 20 : tamanoPagina;

        var query = db.CuentasPorPagar.Where(c => c.SucursalId == sucursalId);
        if (!incluirPagadas)
            query = query.Where(c => c.Estado != EstadoCuenta.Pagada);

        var consulta =
            from c in query
            join pr in db.Proveedores on c.ProveedorId equals pr.Id
            join u in db.Usuarios on c.CreadoPorUsuarioId equals u.Id into usuarios
            from u in usuarios.DefaultIfEmpty()
            select new { Cuenta = c, Proveedor = pr, Usuario = u };

        if (!string.IsNullOrWhiteSpace(texto))
            consulta = consulta.Where(x =>
                EF.Functions.Like(x.Proveedor.NombreORazonSocial, $"%{texto}%") ||
                EF.Functions.Like(x.Cuenta.DocumentoReferencia, $"%{texto}%") ||
                (x.Proveedor.Rnc != null && EF.Functions.Like(x.Proveedor.Rnc, $"%{texto}%")));

        var total = await consulta.CountAsync(ct);

        var items = await (
                from x in consulta
                let c = x.Cuenta
                let pr = x.Proveedor
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
                    Estado = c.Estado,
                    RegistradaPorNombre = x.Usuario != null ? x.Usuario.Nombre : "—"
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
                join u in db.Usuarios on p.CreadoPorUsuarioId equals u.Id
                join ua in db.Usuarios on p.AnuladoPorUsuarioId equals ua.Id into anuladores
                from ua in anuladores.DefaultIfEmpty()
                where p.CuentaPorPagarId == cuentaId
                orderby p.FechaPago descending
                select new PagoCuentaDto
                {
                    Id = p.Id,
                    FechaPago = p.FechaPago,
                    Monto = p.Monto,
                    MetodoPagoNombre = m.Nombre,
                    NumeroComprobante = p.NumeroComprobante,
                    RegistradoPorNombre = u.Nombre,
                    Anulado = p.Anulado,
                    FechaAnulacion = p.FechaAnulacion,
                    AnuladoPorNombre = ua != null ? ua.Nombre : null,
                    MotivoAnulacion = p.MotivoAnulacion
                }
            )
            .ToListAsync(ct);
    }

    public async Task RegistrarPagoCxPAsync(Guid sucursalId, Guid cuentaId, Guid usuarioId, RegistrarPagoRequestDto request, CancellationToken ct = default)
    {
        var cuenta = await db.CuentasPorPagar.FirstOrDefaultAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cuenta por pagar no existe.");

        if (request.Monto <= 0 || request.Monto > cuenta.SaldoPendiente)
            throw new InvalidOperationException("El monto del pago no es válido para el saldo pendiente.");

        var metodoPago = await db.MetodosPago.FirstOrDefaultAsync(m => m.Id == request.MetodoPagoId, ct)
            ?? throw new InvalidOperationException("El método de pago no existe.");

        if (metodoPago.RequiereComprobante && string.IsNullOrWhiteSpace(request.NumeroComprobante))
            throw new InvalidOperationException($"El método de pago \"{metodoPago.Nombre}\" requiere número de comprobante.");

        db.PagosCxP.Add(new PagoCxP
        {
            CuentaPorPagarId = cuenta.Id,
            Monto = request.Monto,
            MetodoPagoId = request.MetodoPagoId,
            CreadoPorUsuarioId = usuarioId,
            NumeroComprobante = metodoPago.RequiereComprobante ? request.NumeroComprobante : null
        });

        cuenta.SaldoPendiente -= request.Monto;
        cuenta.Estado = cuenta.SaldoPendiente == 0 ? EstadoCuenta.Pagada : EstadoCuenta.PagadaParcial;

        await db.SaveChangesAsync(ct);
    }

    // No se borra el pago: se marca Anulado (con motivo y quién/cuándo) y se devuelve el
    // monto al saldo pendiente de la cuenta, preservando el historial para auditoría.
    public async Task AnularPagoCxPAsync(Guid sucursalId, Guid cuentaId, Guid pagoId, Guid usuarioId, AnularPagoRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new InvalidOperationException("El motivo de la anulación es obligatorio.");

        var cuenta = await db.CuentasPorPagar.FirstOrDefaultAsync(c => c.Id == cuentaId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La cuenta por pagar no existe.");

        var pago = await db.PagosCxP.FirstOrDefaultAsync(p => p.Id == pagoId && p.CuentaPorPagarId == cuentaId, ct)
            ?? throw new InvalidOperationException("El pago no existe.");

        if (pago.Anulado)
            throw new InvalidOperationException("Este pago ya fue anulado.");

        pago.Anulado = true;
        pago.FechaAnulacion = DateTime.UtcNow;
        pago.AnuladoPorUsuarioId = usuarioId;
        pago.MotivoAnulacion = request.Motivo;

        cuenta.SaldoPendiente += pago.Monto;
        cuenta.Estado = cuenta.SaldoPendiente == cuenta.MontoOriginal ? EstadoCuenta.Pendiente : EstadoCuenta.PagadaParcial;

        await db.SaveChangesAsync(ct);
        await auditoria.RegistrarAsync(sucursalId, usuarioId, "AnulacionPago", "PagoCxP", pago.Id, request.Motivo, ct);
    }
}
