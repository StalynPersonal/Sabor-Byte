using SaborByte.Dominio.Catalogo;

namespace SaborByte.Dominio.Gastos;

// Catálogo global (no por sucursal), igual patrón que Categoria/UnidadMedida — Admin lo
// administra desde Central sin tocar código.
public class CategoriaGasto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public bool Activo { get; set; } = true;
}

public class Gasto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public DateTime FechaGasto { get; set; }

    public Guid CategoriaGastoId { get; set; }
    public CategoriaGasto? CategoriaGasto { get; set; }

    public required string Descripcion { get; set; }
    public decimal Monto { get; set; }

    // Si está apagado, no cuenta para la ganancia neta del negocio — para gastos que
    // alguien registra aquí solo para llevar cuenta, sin que sean del negocio.
    public bool EsGastoDelNegocio { get; set; } = true;

    public Guid MetodoPagoId { get; set; }
    public MetodoPago? MetodoPago { get; set; }

    // Solo si se pagó en efectivo y había un turno de caja abierto en ese momento — enlaza
    // con el MovimientoCaja tipo Salida creado, para que el cuadre de esa caja no muestre
    // una diferencia sin explicar (ver CajaAppService.RegistrarGastoEfectivoAsync).
    public Guid? TurnoCajaId { get; set; }
    public Guid? MovimientoCajaId { get; set; }

    public Guid CreadoPorUsuarioId { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // No se borra: se marca Anulado (con motivo y quién/cuándo) — igual patrón que
    // PagoCxC/AbonoDelivery, preserva el historial para auditoría y revierte el
    // MovimientoCaja asociado si lo había.
    public bool Anulado { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public Guid? AnuladoPorUsuarioId { get; set; }
    public string? MotivoAnulacion { get; set; }
}
