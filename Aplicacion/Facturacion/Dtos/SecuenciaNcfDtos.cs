namespace SaborByte.Aplicacion.Facturacion.Dtos;

public class SecuenciaNcfDto
{
    public Guid Id { get; set; }
    public required string Serie { get; set; }
    public required string TipoComprobante { get; set; }
    public long SecuenciaInicial { get; set; }
    public long SecuenciaProxima { get; set; }
    public long SecuenciaFinal { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Activa { get; set; }
}

public class GuardarSecuenciaNcfRequestDto
{
    public string Serie { get; set; } = "E";
    public required string TipoComprobante { get; set; }
    public long SecuenciaInicial { get; set; }
    public long SecuenciaProxima { get; set; }
    public long SecuenciaFinal { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Activa { get; set; } = true;
}
