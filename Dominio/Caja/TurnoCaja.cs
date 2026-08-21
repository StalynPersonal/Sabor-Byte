namespace SaborByte.Dominio.Caja;

public enum EstadoTurnoCaja
{
    Abierto,
    Cerrado
}

public class TurnoCaja
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CajaId { get; set; }
    public Caja? Caja { get; set; }

    public Guid UsuarioAperturaId { get; set; }
    public Guid? UsuarioCierreId { get; set; }

    public DateTime FechaHoraApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaHoraCierre { get; set; }

    public decimal MontoAperturaEfectivo { get; set; }
    public EstadoTurnoCaja Estado { get; set; } = EstadoTurnoCaja.Abierto;

    public ICollection<MovimientoCaja> Movimientos { get; set; } = [];
    public ICollection<DenominacionCierre> DenominacionesCierre { get; set; } = [];
}
