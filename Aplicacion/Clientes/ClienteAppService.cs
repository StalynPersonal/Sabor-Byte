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
                c.RncOCedula == texto);
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

    public async Task<Guid> CrearAsync(Guid sucursalId, GuardarClienteRequestDto request, CancellationToken ct = default)
    {
        var cliente = new Cliente
        {
            SucursalId = sucursalId,
            NombreORazonSocial = request.NombreORazonSocial,
            RncOCedula = request.RncOCedula,
            Telefono = request.Telefono,
            Email = request.Email,
            Direccion = request.Direccion,
            TipoCliente = request.TipoCliente
        };

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        return cliente.Id;
    }

    public async Task ActualizarAsync(Guid clienteId, GuardarClienteRequestDto request, CancellationToken ct = default)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, ct)
            ?? throw new InvalidOperationException("El cliente no existe.");

        cliente.NombreORazonSocial = request.NombreORazonSocial;
        cliente.RncOCedula = request.RncOCedula;
        cliente.Telefono = request.Telefono;
        cliente.Email = request.Email;
        cliente.Direccion = request.Direccion;
        cliente.TipoCliente = request.TipoCliente;

        await db.SaveChangesAsync(ct);
    }

    public async Task DesactivarAsync(Guid clienteId, CancellationToken ct = default)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, ct)
            ?? throw new InvalidOperationException("El cliente no existe.");

        cliente.Activo = false;
        await db.SaveChangesAsync(ct);
    }
}
