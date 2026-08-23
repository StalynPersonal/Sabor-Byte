namespace SaborByte.Dominio.Facturacion;

public enum EstadoDgii
{
    NoAplica, // EcfActivo = false para esta venta: NCF tradicional o sin NCF
    Pendiente,
    Aceptado,
    Rechazado,
    Contingencia
}

public class Factura
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public Guid CajaTurnoId { get; set; }

    // Obligatorio: toda factura tiene un cliente, aunque sea el genérico "Cliente Contado"
    // de la sucursal (ver Cliente.EsGenerico) cuando la venta no identificó uno real.
    public Guid ClienteId { get; set; }

    // Snapshot del cliente al momento de la venta — si el cliente se edita después
    // (cambia de nombre, RNC, etc.), la factura ya emitida no debe cambiar de significado
    // retroactivamente (mismo criterio que FacturaDetalle con el producto).
    public required string ClienteNombre { get; set; }
    public string? ClienteRncOCedula { get; set; }

    // Snapshot de dónde salió la venta (igual que TurnoCaja.CodigoSucursal/CodigoCaja):
    // evita tener que hacer join con Cajas/Sucursales solo para saber de dónde es cada
    // factura, y queda fijo aunque el código de la caja o sucursal cambie después.
    public string? SucursalCodigo { get; set; }
    public string? CajaCodigo { get; set; }

    public Guid? ComandaId { get; set; }

    // Número interno correlativo, independiente del NCF fiscal (que solo existe si hay una
    // secuencia NCF activa): CodigoSucursal(2) + CodigoCaja(2) + Secuencia(5) = 9 dígitos,
    // ej. "010100001". Siempre se asigna, tenga o no tenga NCF, para poder identificar
    // cualquier venta del día a día sin depender del estado fiscal de la sucursal.
    public string? NumeroFactura { get; set; }

    public string? NumeroNcf { get; set; }
    public string? TipoComprobante { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Itbis { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }

    public EstadoDgii EstadoDgii { get; set; } = EstadoDgii.NoAplica;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

    // Solo se llenan cuando EcfActivo=true para la sucursal (e-CF real, no NCF tradicional).
    public string? XmlFirmadoDgii { get; set; }
    public string? CodigoSeguridadDgii { get; set; }
    public string? TrackIdDgii { get; set; }

    // Última respuesta de DGII (acuse de recibo o consulta de estado) — sin esto, un
    // rechazo solo se veía una vez en pantalla al momento de facturar y se perdía para
    // siempre si nadie lo leía a tiempo (ver ComprobanteEcf en el plan, sección 2).
    public string? MensajeDgii { get; set; }
    public DateTime? FechaEnvioDgii { get; set; }
    public DateTime? FechaRespuestaDgii { get; set; }

    public Guid CreadoPorUsuarioId { get; set; }

    public ICollection<FacturaDetalle> Detalle { get; set; } = [];
    public ICollection<FacturaPago> Pagos { get; set; } = [];
}
