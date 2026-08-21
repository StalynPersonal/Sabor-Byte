namespace FacturacionElectronicaDGII.Modelos;

// Modelo propio de la librería, desacoplado del dominio del sistema anfitrión.
// Cualquier sistema que integre esta librería solo necesita mapear sus datos a este DTO.
public class ComprobanteDto
{
    public required string TipoNcf { get; set; } // ej. E31, E32, E33...
    public required string NumeroNcf { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimientoSecuencia { get; set; }

    public required EmisorDto Emisor { get; set; }
    public CompradorDto? Comprador { get; set; }

    public List<LineaComprobanteDto> Detalle { get; set; } = [];

    public decimal Subtotal { get; set; }
    public decimal MontoImpuestos { get; set; }
    public decimal Total { get; set; }
}

public class EmisorDto
{
    public required string Rnc { get; set; }
    public required string RazonSocial { get; set; }
}

public class CompradorDto
{
    public string? RncOCedula { get; set; }
    public string? NombreORazonSocial { get; set; }
}

public class LineaComprobanteDto
{
    public required string Descripcion { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
}
