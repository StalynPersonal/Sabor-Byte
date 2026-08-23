namespace SaborByte.Web.Api.Dtos;

public class SecuenciaNcfDto
{
    public Guid Id { get; set; }
    public string Serie { get; set; } = "E";
    public string TipoComprobante { get; set; } = string.Empty;
    public long SecuenciaInicial { get; set; }
    public long SecuenciaProxima { get; set; }
    public long SecuenciaFinal { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Activa { get; set; }
}

public class GuardarSecuenciaNcfRequestDto
{
    public string Serie { get; set; } = "E";
    public string TipoComprobante { get; set; } = string.Empty;
    public long SecuenciaInicial { get; set; }
    public long SecuenciaProxima { get; set; }
    public long SecuenciaFinal { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Activa { get; set; } = true;
}
