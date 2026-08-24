namespace SaborByte.Aplicacion.Facturacion.Dtos;

public class ItemVentaDto
{
    public Guid ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public List<Guid> IngredientesExcluidosIds { get; set; } = [];
    public decimal Descuento { get; set; }
}

public class PagoVentaRequestDto
{
    public Guid MetodoPagoId { get; set; }
    public decimal Monto { get; set; }

    // Solo aplica cuando el método de pago tiene RequiereComprobante (ej. Tarjeta): número
    // de comprobante/autorización que emite el datáfono al procesar el cobro. Opcional.
    public string? NumeroComprobante { get; set; }
}

public class CrearVentaRequestDto
{
    public Guid TurnoCajaId { get; set; }
    public Guid? ClienteId { get; set; }

    // Una factura puede pagarse con más de una forma de pago a la vez (ej. mitad
    // efectivo, mitad tarjeta) — la suma debe coincidir con el Total de la venta.
    public List<PagoVentaRequestDto> Pagos { get; set; } = [];

    // Si se factura desde una comanda existente (mesero -> cocina -> caja), los items
    // se toman de la comanda (excluyendo los cancelados) y Items debe venir vacío —
    // el inventario ya se descontó al enviar la comanda a cocina, no se vuelve a descontar.
    public Guid? ComandaId { get; set; }

    public List<ItemVentaDto> Items { get; set; } = [];

    // Propina: se acepta un % sugerido (ej. 10) o un monto fijo; si ambos vienen,
    // el monto fijo tiene prioridad. El reparto entre meseros queda fuera de v1.
    public decimal? PorcentajePropina { get; set; }
    public decimal? MontoPropinaFijo { get; set; }

    // Obligatorio si algún item trae Descuento > 0 (ver sección 7 del plan:
    // los descuentos solo los autoriza un Supervisor/Admin).
    public Guid? CodigoAutorizacionDescuento { get; set; }
}

public class VentaResultadoDto
{
    public Guid FacturaId { get; set; }
    public string? NumeroFactura { get; set; }
    public string? NumeroNcf { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Itbis { get; set; }
    public decimal Descuento { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaEmision { get; set; }
    public List<PagoVentaRequestDto> Pagos { get; set; } = [];

    // Solo aplica cuando se pagó completo con una única forma de pago en efectivo y se
    // recibió de más — ver VentaAppService.ValidarPagosYCalcularCambio.
    public decimal Cambio { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteRncOCedula { get; set; }
    public string? CajeroNombre { get; set; }
    public string? CodigoSeguridadDgii { get; set; }

    // Solo se llena si la sucursal tiene e-CF activo y se intentó enviar a DGII —
    // resultado o motivo del fallo, informativo (nunca bloqueó la venta).
    public string? MensajeDgii { get; set; }
}
