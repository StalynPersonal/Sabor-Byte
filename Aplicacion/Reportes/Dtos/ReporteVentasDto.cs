namespace SaborByte.Aplicacion.Reportes.Dtos;

public class ReporteVentasRequestDto
{
    public List<Guid> SucursalesIds { get; set; } = [];
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
}

public class ReporteVentasPorSucursalDto
{
    public Guid SucursalId { get; set; }
    public string NombreSucursal { get; set; } = string.Empty;
    public int CantidadFacturas { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal TotalItbis { get; set; }
    public decimal TicketPromedio { get; set; }
}

public class ReporteVentasConsolidadoDto
{
    public List<ReporteVentasPorSucursalDto> PorSucursal { get; set; } = [];
    public decimal TotalConsolidado { get; set; }
}

public class RangoFechasRequestDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
}

public class VentaPorProductoDto
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
    // null si el producto no tiene CostoUnitario configurado (no se puede estimar utilidad).
    public decimal? UtilidadEstimada { get; set; }
}

public class VentaPorHoraDto
{
    public int Hora { get; set; } // 0-23
    public int CantidadFacturas { get; set; }
    public decimal TotalVendido { get; set; }
}
