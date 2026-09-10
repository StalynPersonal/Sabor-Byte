using SaborByte.Dominio.Catalogo;

namespace SaborByte.Dominio.Deliveries;

public class Delivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public required string Nombre { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;

    // Cacheado (igual que CuentaPorCobrar.SaldoPendiente): suma de MontoFactura de las
    // facturas asignadas menos los abonos vigentes, actualizado en cada mutación en vez
    // de recalcularse por SUM en cada lectura.
    public decimal SaldoPendiente { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public Guid CreadoPorUsuarioId { get; set; }

    public ICollection<FacturaDelivery> Facturas { get; set; } = [];
    public ICollection<AbonoDelivery> Abonos { get; set; } = [];
}

public class FacturaDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryId { get; set; }
    public Delivery? Delivery { get; set; }
    public Guid FacturaId { get; set; }
    public Guid SucursalId { get; set; }

    // Snapshot del Total de la factura al momento de asignarla (mismo criterio que
    // Factura.ClienteNombre): si la factura se acredita después, el vínculo ya registrado
    // no cambia de significado retroactivamente.
    public decimal MontoFactura { get; set; }

    // Cobro informativo del flete/viaje que el delivery retiene para sí — no forma parte
    // del SaldoPendiente del delivery (eso es solo lo que debe devolver de productos).
    public decimal MontoDelivery { get; set; }

    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public Guid AsignadoPorUsuarioId { get; set; }
}

public class AbonoDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryId { get; set; }
    public Delivery? Delivery { get; set; }
    public Guid SucursalId { get; set; }

    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public Guid MetodoPagoId { get; set; }
    public MetodoPago? MetodoPago { get; set; }

    // Solo aplica cuando MetodoPago.RequiereComprobante (ej. Transferencia, Depósito).
    public string? NumeroComprobante { get; set; }
    public Guid CreadoPorUsuarioId { get; set; }

    // No se borra el abono: se marca Anulado (con motivo y quién/cuándo) y se revierte el
    // saldo pendiente del delivery, preservando el historial para auditoría — mismo patrón
    // que PagoCxC/PagoCxP.
    public bool Anulado { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public Guid? AnuladoPorUsuarioId { get; set; }
    public string? MotivoAnulacion { get; set; }
}
