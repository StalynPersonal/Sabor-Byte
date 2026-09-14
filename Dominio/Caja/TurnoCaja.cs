namespace SaborByte.Dominio.Caja;

public enum EstadoTurnoCaja
{
    Abierto,
    Cerrado
}

public class TurnoCaja
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Identificador corto y legible para el cajero/administrador (ej. "Turno #142"),
    // a diferencia del Id (GUID) que no es práctico de comunicar de palabra o por escrito.
    // Autogenerado por la base de datos (columna IDENTITY), único en todo el sistema.
    public int NumeroTurno { get; set; }

    public Guid CajaId { get; set; }
    public Caja? Caja { get; set; }

    // Snapshot tomado al abrir el turno (no join en caliente): si el código de la caja o
    // de la sucursal cambia después, este turno histórico no debe cambiar de significado.
    public string? CodigoSucursal { get; set; }
    public string? CodigoCaja { get; set; }

    public Guid UsuarioAperturaId { get; set; }
    public Guid? UsuarioCierreId { get; set; }

    // IP real de la máquina que abrió este turno (capturada del lado del servidor, no del
    // request — no se puede falsear). Guardada aparte de Caja.IpPermitida porque esa es la
    // configuración esperada, esta es lo que realmente pasó en ese turno puntual.
    public string? IpApertura { get; set; }

    public DateTime FechaHoraApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaHoraCierre { get; set; }

    public decimal MontoAperturaEfectivo { get; set; }
    public EstadoTurnoCaja Estado { get; set; } = EstadoTurnoCaja.Abierto;

    public ICollection<MovimientoCaja> Movimientos { get; set; } = [];
    public ICollection<DenominacionCierre> DenominacionesCierre { get; set; } = [];
}
