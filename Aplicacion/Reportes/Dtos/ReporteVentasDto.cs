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
