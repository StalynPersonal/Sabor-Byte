using SaborByte.Dominio.Caja;

namespace SaborByte.Aplicacion.Caja.Dtos;

public class AbrirTurnoRequestDto
{
    public Guid CajaId { get; set; }
    public decimal MontoAperturaEfectivo { get; set; }
    public string IpOrigen { get; set; } = string.Empty; // el backend la completa con la IP real del request
    public string? HostnameOrigen { get; set; }
}

public class DenominacionCierreDto
{
    public FormaPago FormaPago { get; set; }
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
    public FormaPago FormaPago { get; set; }
    public decimal Esperado { get; set; }
    public decimal Contado { get; set; }
    public decimal Diferencia { get; set; }
}

public class ResumenTurnoDto
{
    public Guid TurnoCajaId { get; set; }
    public EstadoTurnoCaja Estado { get; set; }
    public DateTime FechaHoraApertura { get; set; }
    public DateTime? FechaHoraCierre { get; set; }
    public decimal MontoAperturaEfectivo { get; set; }
    public List<TotalPorFormaPagoDto> Totales { get; set; } = [];
}
