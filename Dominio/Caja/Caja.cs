namespace SaborByte.Dominio.Caja;

public class Caja
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }

    // Copia del código de la sucursal dueña (Sucursal.Codigo), mantenida al día por
    // CajaAppService al crear/actualizar la caja — evita tener que hacer join con
    // Sucursales solo para mostrar o reportar de qué sucursal es cada caja.
    public string? CodigoSucursal { get; set; }

    public required string Numero { get; set; }
    public bool Activa { get; set; } = true;

    // Seguridad: la caja solo puede abrirse desde esta máquina física.
    public string? IpPermitida { get; set; }
    public string? HostnamePermitido { get; set; }

    // Contador de la parte "secuencia" de Factura.NumeroFactura, exclusivo de esta caja.
    // Se avanza con compare-and-swap (igual que SecuenciaNcf.SecuenciaProxima) para que
    // dos ventas concurrentes en la misma caja nunca reciban el mismo número.
    public long ProximoNumeroFactura { get; set; } = 1;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    // Nullable: la caja de siembra inicial no tiene un usuario autenticado que la haya creado.
    public Guid? CreadoPorUsuarioId { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public Guid? ActualizadoPorUsuarioId { get; set; }
}
