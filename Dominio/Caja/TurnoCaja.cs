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

    // Snapshot tomado al abrir el turno (no join en caliente): si el código de la caja o
    // de la sucursal cambia después, este turno histórico no debe cambiar de significado.
    public string? CodigoSucursal { get; set; }
    public string? CodigoCaja { get; set; }

    public Guid UsuarioAperturaId { get; set; }
    public Guid? UsuarioCierreId { get; set; }

    public DateTime FechaHoraApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaHoraCierre { get; set; }

    public decimal MontoAperturaEfectivo { get; set; }
    public EstadoTurnoCaja Estado { get; set; } = EstadoTurnoCaja.Abierto;

    public ICollection<MovimientoCaja> Movimientos { get; set; } = [];
    public ICollection<DenominacionCierre> DenominacionesCierre { get; set; } = [];
}
