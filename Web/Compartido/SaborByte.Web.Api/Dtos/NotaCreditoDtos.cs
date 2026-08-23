namespace SaborByte.Web.Api.Dtos;

public class MotivoNotaCreditoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class GuardarMotivoNotaCreditoRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public class FacturaDetalleDisponibleDto
{
    public Guid FacturaDetalleId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CantidadAcreditada { get; set; }
    public decimal CantidadDisponible { get; set; }
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
    public Guid CodigoAutorizacion { get; set; }
}

public class NotaCreditoDetalleDto
{
    public Guid FacturaDetalleId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class NotaCreditoDto
{
    public Guid Id { get; set; }
    public Guid FacturaOriginalId { get; set; }
    public string? NumeroNota { get; set; }
    public string? NumeroFacturaOriginal { get; set; }
    public string? NumeroNcf { get; set; }
    public string Motivo { get; set; } = string.Empty;
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
