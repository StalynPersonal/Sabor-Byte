namespace SaborByte.Aplicacion.Deliveries.Dtos;

public class DeliveryDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal? LimiteSaldoPendiente { get; set; }

    // Días desde la factura asignada más antigua que los abonos (aplicados FIFO) todavía
    // no alcanzan a cubrir — null si el saldo pendiente es 0.
    public int? DiasAntiguedadSaldo { get; set; }
}

public class GuardarDeliveryRequestDto
{
    public required string Nombre { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
    public decimal? LimiteSaldoPendiente { get; set; }
}

public class FacturaAsignableDto
{
    public Guid FacturaId { get; set; }
    public string? NumeroFactura { get; set; }
    public string? NumeroNcf { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
    public bool YaAsignada { get; set; }
    public string? DeliveryNombreActual { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;

    // Teléfono ACTUAL del cliente (no un snapshot de la factura, que no lo guarda) — para
    // el aviso por WhatsApp interesa el número vigente, no el que tenía al facturar.
    public string? ClienteTelefono { get; set; }
}

public class AsignarFacturaRequestDto
{
    public Guid FacturaId { get; set; }
    public decimal MontoDelivery { get; set; }
}

public class FacturaDeliveryDto
{
    public Guid Id { get; set; }
    public Guid FacturaId { get; set; }
    public string? NumeroFactura { get; set; }
    public string? NumeroNcf { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal MontoFactura { get; set; }
    public decimal MontoDelivery { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public string AsignadoPorNombre { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteTelefono { get; set; }
    public bool Quitada { get; set; }
    public DateTime? FechaQuitada { get; set; }
    public string? QuitadoPorNombre { get; set; }
    public string? MotivoQuitar { get; set; }
}

public class QuitarAsignacionRequestDto
{
    public required string Motivo { get; set; }
}

public class ReporteDesempenoDeliveryDto
{
    public Guid DeliveryId { get; set; }
    public string DeliveryNombre { get; set; } = string.Empty;
    public int CantidadEntregas { get; set; }
    public decimal MontoTotalFacturado { get; set; }
    public decimal MontoTotalAbonado { get; set; }
    public decimal SaldoPendienteActual { get; set; }
    public int? DiasAntiguedadSaldo { get; set; }
}

public class AbonoDeliveryDto
{
    public Guid Id { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string MetodoPagoNombre { get; set; } = string.Empty;
    public string? NumeroComprobante { get; set; }
    public string RegistradoPorNombre { get; set; } = string.Empty;
    public bool Anulado { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? AnuladoPorNombre { get; set; }
    public string? MotivoAnulacion { get; set; }
}

public class RegistrarAbonoDeliveryRequestDto
{
    public decimal Monto { get; set; }
    public Guid MetodoPagoId { get; set; }
    public string? NumeroComprobante { get; set; }
}

public class AnularAbonoDeliveryRequestDto
{
    public required string Motivo { get; set; }
}

// Totales de toda la vida de la cuenta (no del rango/página que se esté viendo en las
// tablas paginadas) — para las tarjetas de resumen del diálogo "Cuenta".
public class ResumenCuentaDeliveryDto
{
    public decimal TotalFacturado { get; set; }
    public decimal TotalAbonado { get; set; }
}
