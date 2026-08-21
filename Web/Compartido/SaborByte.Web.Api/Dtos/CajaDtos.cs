namespace SaborByte.Web.Api.Dtos;

public enum FormaPago
{
    Efectivo,
    Tarjeta,
    Transferencia,
    Deposito
}

public class CajaResumenDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public bool Activa { get; set; }
}

public class AbrirTurnoRequestDto
{
    public Guid CajaId { get; set; }
    public decimal MontoAperturaEfectivo { get; set; }
}

public class AbrirTurnoResponseDto
{
    public Guid TurnoCajaId { get; set; }
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
    public int Estado { get; set; }
    public DateTime FechaHoraApertura { get; set; }
    public DateTime? FechaHoraCierre { get; set; }
    public decimal MontoAperturaEfectivo { get; set; }
    public List<TotalPorFormaPagoDto> Totales { get; set; } = [];
}
