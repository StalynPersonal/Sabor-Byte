using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Clientes.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Clientes;

namespace SaborByte.Aplicacion.Clientes;

public class ClienteAppService(IAppDbContext db)
{
    public async Task<List<ClienteDto>> BuscarAsync(Guid sucursalId, string? texto, CancellationToken ct = default)
    {
        var query = db.Clientes.Where(c => c.SucursalId == sucursalId && c.Activo);

        if (!string.IsNullOrWhiteSpace(texto))
        {
            query = query.Where(c =>
                EF.Functions.Like(c.NombreORazonSocial, $"%{texto}%") ||
                (c.RncOCedula != null && EF.Functions.Like(c.RncOCedula, $"%{texto}%")));
        }

        return await query
            .OrderBy(c => c.NombreORazonSocial)
            .Take(50)
            .Select(c => new ClienteDto
            {
                Id = c.Id,
                NombreORazonSocial = c.NombreORazonSocial,
                RncOCedula = c.RncOCedula,
                Telefono = c.Telefono,
                Email = c.Email,
                Direccion = c.Direccion,
                TipoCliente = c.TipoCliente,
                Activo = c.Activo
            })
            .ToListAsync(ct);
    }

    public async Task<Guid> CrearAsync(Guid sucursalId, Guid usuarioId, GuardarClienteRequestDto request, CancellationToken ct = default)
    {
        await ValidarAsync(sucursalId, request, clienteId: null, ct);

        var cliente = new Cliente
        {
            SucursalId = sucursalId,
            NombreORazonSocial = request.NombreORazonSocial.Trim(),
            RncOCedula = NormalizarRnc(request.RncOCedula),
            Telefono = request.Telefono,
            Email = request.Email,
            Direccion = request.Direccion,
            TipoCliente = request.TipoCliente,
            CreadoPorUsuarioId = usuarioId
        };

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        return cliente.Id;
    }

    public async Task ActualizarAsync(Guid sucursalId, Guid clienteId, GuardarClienteRequestDto request, CancellationToken ct = default)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El cliente no existe.");

        await ValidarAsync(sucursalId, request, clienteId, ct);

        cliente.NombreORazonSocial = request.NombreORazonSocial.Trim();
        cliente.RncOCedula = NormalizarRnc(request.RncOCedula);
        cliente.Telefono = request.Telefono;
        cliente.Email = request.Email;
        cliente.Direccion = request.Direccion;
        cliente.TipoCliente = request.TipoCliente;

        await db.SaveChangesAsync(ct);
    }

    public async Task DesactivarAsync(Guid sucursalId, Guid clienteId, CancellationToken ct = default)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId && c.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("El cliente no existe.");

        cliente.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    private static string? NormalizarRnc(string? rncOCedula) =>
        string.IsNullOrWhiteSpace(rncOCedula) ? null : rncOCedula.Trim();

    private async Task ValidarAsync(Guid sucursalId, GuardarClienteRequestDto request, Guid? clienteId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NombreORazonSocial))
            throw new InvalidOperationException("El nombre o razón social del cliente es obligatorio.");

        var rnc = NormalizarRnc(request.RncOCedula);

        if (request.TipoCliente == TipoCliente.Fiscal && rnc is null)
            throw new InvalidOperationException("El RNC/Cédula es obligatorio para un cliente Fiscal.");

        if (rnc is not null)
        {
            var yaExiste = await db.Clientes.AnyAsync(c =>
                c.SucursalId == sucursalId && c.RncOCedula == rnc && c.Id != clienteId, ct);

            if (yaExiste)
                throw new InvalidOperationException($"Ya existe otro cliente con el RNC/Cédula '{rnc}' en esta sucursal.");
        }
    }
}
