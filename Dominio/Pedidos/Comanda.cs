namespace SaborByte.Dominio.Pedidos;

public enum EstadoComanda
{
    Abierta,
    EnviadaCocina,
    Cerrada,
    Cancelada
}

public enum OrigenCreacion
{
    Online,
    Offline
}

public class Comanda
{
    // Id generado en cliente (GUID) — ver sección 1.3 del plan: es la clave definitiva,
    // no un autoincremental de servidor, para soportar creación fuera de línea en el futuro.
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }

    public int NumeroComanda { get; set; } // correlativo visible, asignado por el servidor
    public Guid? MesaId { get; set; }
    public Guid? MeseroId { get; set; } // null si se tomó directo en caja

    public EstadoComanda Estado { get; set; } = EstadoComanda.Abierta;
    public OrigenCreacion OrigenCreacion { get; set; } = OrigenCreacion.Online;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public ICollection<ComandaItem> Items { get; set; } = [];
}
