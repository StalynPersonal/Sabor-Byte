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

public class VentaResumenDiaDto
{
    public DateTime Fecha { get; set; }
    public int CantidadFacturas { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal TotalItbis { get; set; }
    public decimal TicketPromedio { get; set; }
}

public class VentaDetalleDto
{
    public Guid FacturaId { get; set; }
    public string? NumeroFactura { get; set; }
    public string? NumeroNcf { get; set; }
    public DateTime FechaEmision { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Itbis { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string FormasPago { get; set; } = string.Empty;
    public string CajeroNombre { get; set; } = string.Empty;
    // null si la venta se facturó directo en Caja, sin pasar por Mesero.
    public string? MeseroNombre { get; set; }
}

// Resumen por cajero — para la pestaña "Ventas por Cajero".
public class VentaPorCajeroDto
{
    public Guid CajeroId { get; set; }
    public string NombreCajero { get; set; } = string.Empty;
    public int CantidadFacturas { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal TicketPromedio { get; set; }
}

// Resumen por mesero — para la pestaña "Ventas por Mesero". Solo cuenta ventas que
// pasaron por una comanda tomada por un mesero (las facturadas directo en Caja no
// tienen mesero y quedan fuera de este resumen).
public class VentaPorMeseroDto
{
    public Guid MeseroId { get; set; }
    public string NombreMesero { get; set; } = string.Empty;
    public int CantidadFacturas { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal TicketPromedio { get; set; }
}

public class VentaPorMetodoPagoDto
{
    public Guid MetodoPagoId { get; set; }
    public string NombreMetodo { get; set; } = string.Empty;
    public int CantidadPagos { get; set; }
    public decimal TotalCobrado { get; set; }
}

public class MovimientoInventarioReporteDto
{
    public DateTime FechaHora { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal SaldoResultante { get; set; }
    public string? Nota { get; set; }
}

public class VentaPorCategoriaDto
{
    public Guid CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal TotalVendido { get; set; }
}

public class CuentaPendienteDto
{
    public Guid CuentaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class PagoCuentaReporteDto
{
    public Guid PagoId { get; set; }
    public string Nombre { get; set; } = string.Empty; // Cliente o Proveedor
    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPagoNombre { get; set; } = string.Empty;
    public string? NumeroComprobante { get; set; }
    public string RegistradoPorNombre { get; set; } = string.Empty;
    public bool Anulado { get; set; }
}

public class DashboardResumenDto
{
    public decimal VentasHoyTotal { get; set; }
    public int VentasHoyCantidadFacturas { get; set; }
    public decimal TicketPromedioHoy { get; set; }
    public int TurnosAbiertos { get; set; }
    public int CxCPendienteCantidad { get; set; }
    public decimal CxCPendienteTotal { get; set; }
    public int CxPPendienteCantidad { get; set; }
    public decimal CxPPendienteTotal { get; set; }
    public int ProductosStockBajo { get; set; }
    public decimal CobradoHoyCxC { get; set; }
    public decimal PagadoHoyCxP { get; set; }
    public int CuentasVencidasCantidad { get; set; }
    public decimal CuentasVencidasTotal { get; set; }
    public int NotasCreditoHoyCantidad { get; set; }
    public decimal NotasCreditoHoyTotal { get; set; }
    public List<VentaPorHoraDto> VentasPorHoraHoy { get; set; } = [];
    public List<VentaPorProductoDto> TopProductosHoy { get; set; } = [];
}
