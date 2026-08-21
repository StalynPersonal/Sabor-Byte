namespace SaborByte.Dominio.Caja;

public class DenominacionCierre
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TurnoCajaId { get; set; }
    public TurnoCaja? TurnoCaja { get; set; }

    public FormaPago FormaPago { get; set; }

    // Solo aplica cuando FormaPago = Efectivo: valor del billete/moneda (ej. 1000, 500, 100...).
    public decimal? Denominacion { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}
