namespace SaborByte.Web.Api.Dtos;

public class CajaResumenDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public long ProximoNumeroFactura { get; set; }
    public string? CodigoSucursal { get; set; }
}

public class CajaDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string? CodigoSucursal { get; set; }
    public bool Activa { get; set; }
    public string? IpPermitida { get; set; }
    public string? HostnamePermitido { get; set; }
    public long ProximoNumeroFactura { get; set; }
}

public class GuardarCajaRequestDto
{
    public string Numero { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public string? IpPermitida { get; set; }
    public string? HostnamePermitido { get; set; }
    public long ProximoNumeroFactura { get; set; } = 1;
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

public class TurnoAbiertoDto
{
    public Guid TurnoCajaId { get; set; }
    public DateTime FechaHoraApertura { get; set; }
    public decimal MontoAperturaEfectivo { get; set; }
    public string? UsuarioAperturaNombre { get; set; }
    public bool EsDeOtroDia { get; set; }
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
    public string MetodoPagoNombre { get; set; } = string.Empty;
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
