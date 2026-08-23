using SaborByte.Dominio.Catalogo;

namespace SaborByte.Dominio.CxcCxp;

public enum EstadoCuenta
{
    Pendiente,
    PagadaParcial,
    Pagada,
    Vencida
}

public class CuentaPorCobrar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid FacturaId { get; set; }

    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoCuenta Estado { get; set; } = EstadoCuenta.Pendiente;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public Guid CreadoPorUsuarioId { get; set; }

    public ICollection<PagoCxC> Pagos { get; set; } = [];
}

public class PagoCxC
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CuentaPorCobrarId { get; set; }
    public CuentaPorCobrar? Cuenta { get; set; }

    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public Guid CreadoPorUsuarioId { get; set; }
    public Guid MetodoPagoId { get; set; }
    public MetodoPago? MetodoPago { get; set; }

    // Solo aplica cuando MetodoPago.RequiereComprobante (ej. Transferencia, Depósito):
    // número de referencia/autorización del pago.
    public string? NumeroComprobante { get; set; }

    // Anulación: no se borra el pago (queda para auditoría), se marca y se revierte el
    // saldo de la cuenta. Solo Admin/Supervisor puede anular — ver CxcCxpController.
    public bool Anulado { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public Guid? AnuladoPorUsuarioId { get; set; }
    public string? MotivoAnulacion { get; set; }
}
