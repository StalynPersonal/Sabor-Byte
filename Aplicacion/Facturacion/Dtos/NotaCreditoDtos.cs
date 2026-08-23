namespace SaborByte.Aplicacion.Facturacion.Dtos;

public class MotivoNotaCreditoDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public bool Activo { get; set; }
}

public class GuardarMotivoNotaCreditoRequestDto
{
    public required string Nombre { get; set; }
    public bool Activo { get; set; } = true;
}

// Una línea de la factura original, con lo que ya se acreditó y lo que todavía se
// puede devolver — lo que ve el usuario en Central para elegir qué y cuánto acreditar.
public class FacturaDetalleDisponibleDto
{
    public Guid FacturaDetalleId { get; set; }
    public required string NombreProducto { get; set; }
    public string? Codigo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CantidadAcreditada { get; set; }
    public decimal CantidadDisponible => Cantidad - CantidadAcreditada;
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}

public class DetalleNotaRequestDto
{
    public Guid FacturaDetalleId { get; set; }
    public decimal Cantidad { get; set; }
}

public class CrearNotaRequestDto
{
    public Guid FacturaOriginalId { get; set; }
    public Guid MotivoId { get; set; }
    public List<DetalleNotaRequestDto> Detalle { get; set; } = [];

    // Código de un solo uso obtenido de POST /api/autorizaciones (Accion = "EmitirNotaCredito").
    // Toda nota de crédito requiere autorización de Supervisor/Admin, sin excepción.
    public Guid CodigoAutorizacion { get; set; }
}

public class NotaCreditoDetalleDto
{
    public Guid FacturaDetalleId { get; set; }
    public required string NombreProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class NotaCreditoDto
{
    public Guid Id { get; set; }
    public Guid FacturaOriginalId { get; set; }

    // Número interno de la nota (siempre asignado) y N° de factura original al que
    // hace referencia — sin esto había que ir a buscar la factura para saber a cuál
    // aplicaba la nota.
    public string? NumeroNota { get; set; }
    public string? NumeroFacturaOriginal { get; set; }

    public string? NumeroNcf { get; set; }
    public string Motivo { get; set; } = string.Empty;

    // La nota no tiene caja propia (se emite desde Central, no desde un turno de caja) —
    // se hereda de la caja que emitió la factura original, para trazabilidad.
    public string? SucursalCodigo { get; set; }
    public string? CajaNumero { get; set; }

    public decimal Monto { get; set; }
    public DateTime FechaEmision { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteRncOCedula { get; set; }
    public string? CajeroNombre { get; set; }
    public string? CodigoSeguridadDgii { get; set; }

    public List<NotaCreditoDetalleDto> Detalle { get; set; } = [];
}
