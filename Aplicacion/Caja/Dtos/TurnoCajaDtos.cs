using SaborByte.Dominio.Caja;

namespace SaborByte.Aplicacion.Caja.Dtos;

public class CajaDto
{
    public Guid Id { get; set; }
    public required string Numero { get; set; }

    // Código de la sucursal dueña de esta caja — solo informativo (se lee de Sucursal.Codigo,
    // no se duplica en la tabla Cajas), para que se vea de un vistazo cómo va a quedar
    // formado Factura.NumeroFactura (CodigoSucursal + este Numero + la secuencia).
    public string? CodigoSucursal { get; set; }

    public bool Activa { get; set; }
    public string? IpPermitida { get; set; }
    public string? HostnamePermitido { get; set; }

    // Próximo consecutivo que se asignará a Factura.NumeroFactura desde esta caja.
    // Editable por Admin (ej. para saltar un bloque de números), nunca se permite bajarlo
    // por debajo del valor actual porque colisionaría con números ya emitidos.
    public long ProximoNumeroFactura { get; set; }
}

public class GuardarCajaRequestDto
{
    public required string Numero { get; set; }
    public bool Activa { get; set; } = true;
    public string? IpPermitida { get; set; }
    public string? HostnamePermitido { get; set; }
    public long ProximoNumeroFactura { get; set; } = 1;
}

public class AbrirTurnoRequestDto
{
    public Guid CajaId { get; set; }
    public decimal MontoAperturaEfectivo { get; set; }
    public string IpOrigen { get; set; } = string.Empty; // el backend la completa con la IP real del request
    public string? HostnameOrigen { get; set; }
}

public class DenominacionCierreDto
{
    public Guid MetodoPagoId { get; set; }
    public decimal? Denominacion { get; set; }
    public int Cantidad { get; set; }
}

public class CerrarTurnoRequestDto
{
    public Guid TurnoCajaId { get; set; }
    public List<DenominacionCierreDto> Denominaciones { get; set; } = [];
}

public class TotalPorFormaPagoDto
{
    public Guid MetodoPagoId { get; set; }
    public required string MetodoPagoNombre { get; set; }
    public decimal Esperado { get; set; }
    public decimal Contado { get; set; }
    public decimal Diferencia { get; set; }
}

// Turno ya abierto (por cualquier usuario) que se encuentra al elegir una caja — permite
// retomarlo si es del mismo día, o forzar el cierre si quedó abierto de un día anterior.
public class TurnoAbiertoDto
{
    public Guid TurnoCajaId { get; set; }
    public DateTime FechaHoraApertura { get; set; }
    public decimal MontoAperturaEfectivo { get; set; }
    public string? UsuarioAperturaNombre { get; set; }
    public bool EsDeOtroDia { get; set; }
}

public class ResumenTurnoDto
{
    public Guid TurnoCajaId { get; set; }
    public EstadoTurnoCaja Estado { get; set; }
    public string? CajaNumero { get; set; }
    public DateTime FechaHoraApertura { get; set; }
    public DateTime? FechaHoraCierre { get; set; }
    public decimal MontoAperturaEfectivo { get; set; }
    public string? UsuarioAperturaNombre { get; set; }
    public string? UsuarioCierreNombre { get; set; }
    public List<TotalPorFormaPagoDto> Totales { get; set; } = [];
    public List<DetalleDenominacionDto> Denominaciones { get; set; } = [];
}

public class DetalleDenominacionDto
{
    public required string MetodoPagoNombre { get; set; }
    public decimal? Denominacion { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

// Un turno ya cerrado — fila del listado "Cuadres de Caja" en Central.
public class TurnoCerradoResumenDto
{
    public Guid TurnoCajaId { get; set; }
    public required string CajaNumero { get; set; }
    public DateTime FechaHoraApertura { get; set; }
    public DateTime? FechaHoraCierre { get; set; }
    public string? UsuarioAperturaNombre { get; set; }
    public string? UsuarioCierreNombre { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal DiferenciaTotal { get; set; }
}
