namespace SaborByte.Dominio.Caja;

public enum TipoMovimientoCaja
{
    Venta,
    Entrada,
    Salida,
    Ajuste
}

public enum FormaPago
{
    Efectivo,
    Tarjeta,
    Transferencia,
    Deposito
}

public class MovimientoCaja
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TurnoCajaId { get; set; }
    public TurnoCaja? TurnoCaja { get; set; }

    public TipoMovimientoCaja Tipo { get; set; }
    public Guid? FacturaId { get; set; }
    public FormaPago FormaPago { get; set; }
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
