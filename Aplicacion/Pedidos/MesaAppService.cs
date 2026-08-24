using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Aplicacion.Pedidos.Dtos;
using SaborByte.Dominio.Pedidos;

namespace SaborByte.Aplicacion.Pedidos;

public class MesaAppService(IAppDbContext db)
{
    public async Task<List<MesaDto>> ListarAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var mesas = await db.Mesas
            .Where(m => m.SucursalId == sucursalId)
            .OrderBy(m => m.Salon).ThenBy(m => m.Numero)
            .ToListAsync(ct);

        return mesas.Select(MapearMesa).ToList();
    }

    public async Task<Guid> CrearAsync(Guid sucursalId, GuardarMesaRequestDto request, CancellationToken ct = default)
    {
        ValidarDatos(request);
        await ValidarNumeroUnicoEnSalonAsync(sucursalId, request.Numero, request.Salon, mesaId: null, ct);

        var mesa = new Mesa
        {
            SucursalId = sucursalId,
            Numero = request.Numero,
            Salon = request.Salon,
            Capacidad = request.Capacidad
        };

        db.Mesas.Add(mesa);
        await db.SaveChangesAsync(ct);
        return mesa.Id;
    }

    public async Task ActualizarAsync(Guid sucursalId, Guid mesaId, GuardarMesaRequestDto request, CancellationToken ct = default)
    {
        // Filtra por sucursalId (no solo por mesaId) para evitar que un usuario con acceso
        // a su propia sucursal edite una mesa de otra sucursal solo adivinando el GUID.
        var mesa = await db.Mesas.FirstOrDefaultAsync(m => m.Id == mesaId && m.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La mesa no existe.");

        ValidarDatos(request);
        await ValidarNumeroUnicoEnSalonAsync(sucursalId, request.Numero, request.Salon, mesaId, ct);

        mesa.Numero = request.Numero;
        mesa.Salon = request.Salon;
        mesa.Capacidad = request.Capacidad;
        await db.SaveChangesAsync(ct);
    }

    private static void ValidarDatos(GuardarMesaRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Numero))
            throw new InvalidOperationException("El número de mesa es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Salon))
            throw new InvalidOperationException("El salón es obligatorio.");

        if (request.Capacidad <= 0)
            throw new InvalidOperationException("La capacidad debe ser mayor a cero.");
    }

    // Dos mesas del mismo salón no pueden compartir número (dos mesas "05" en el salón
    // Terraza sería ambiguo para el mesero/cliente); mesas en salones distintos sí pueden
    // repetir número.
    private async Task ValidarNumeroUnicoEnSalonAsync(
        Guid sucursalId, string numero, string? salon, Guid? mesaId, CancellationToken ct)
    {
        var yaExiste = await db.Mesas.AnyAsync(m =>
            m.SucursalId == sucursalId && m.Numero == numero && m.Salon == salon && m.Id != mesaId, ct);

        if (yaExiste)
        {
            var descripcionSalon = string.IsNullOrWhiteSpace(salon) ? "sin salón asignado" : $"el salón '{salon}'";
            throw new InvalidOperationException($"Ya existe una mesa con el número '{numero}' en {descripcionSalon}.");
        }
    }

    public async Task LiberarAsync(Guid sucursalId, Guid mesaId, CancellationToken ct = default)
    {
        // Escape manual para el caso borde de una mesa que quedó "Ocupada" por un error
        // (ej. una comanda cancelada sin pasar por el cierre normal) — Central puede
        // forzar la liberación sin tener que tocar la base de datos directamente.
        var mesa = await db.Mesas.FirstOrDefaultAsync(m => m.Id == mesaId && m.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La mesa no existe.");

        mesa.Estado = EstadoMesa.Libre;
        await db.SaveChangesAsync(ct);
    }

    public async Task CambiarActivoAsync(Guid sucursalId, Guid mesaId, bool activo, CancellationToken ct = default)
    {
        var mesa = await db.Mesas.FirstOrDefaultAsync(m => m.Id == mesaId && m.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La mesa no existe.");

        if (!activo && mesa.Estado == EstadoMesa.Ocupada)
            throw new InvalidOperationException("No se puede desactivar una mesa ocupada. Libérala primero.");

        mesa.Activo = activo;
        await db.SaveChangesAsync(ct);
    }

    private static MesaDto MapearMesa(Mesa m) => new()
    {
        Id = m.Id,
        Numero = m.Numero,
        Salon = m.Salon,
        Capacidad = m.Capacidad,
        Estado = m.Estado,
        Activo = m.Activo
    };
}
