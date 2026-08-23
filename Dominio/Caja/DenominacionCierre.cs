using SaborByte.Dominio.Catalogo;

namespace SaborByte.Dominio.Caja;

public class DenominacionCierre
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TurnoCajaId { get; set; }
    public TurnoCaja? TurnoCaja { get; set; }

    // Copiado del TurnoCaja al cerrar — mismo criterio de snapshot que TurnoCaja.CodigoCaja.
    public string? CodigoSucursal { get; set; }
    public string? CodigoCaja { get; set; }

    public Guid MetodoPagoId { get; set; }
    public MetodoPago? MetodoPago { get; set; }

    // Solo aplica cuando MetodoPago.EsEfectivo: valor del billete/moneda (ej. 1000, 500, 100...).
    public decimal? Denominacion { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}
