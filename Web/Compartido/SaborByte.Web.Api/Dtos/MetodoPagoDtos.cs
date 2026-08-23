namespace SaborByte.Web.Api.Dtos;

public class MetodoPagoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsEfectivo { get; set; }
    public bool RequiereComprobante { get; set; }
    public bool Activo { get; set; }
}

public class GuardarMetodoPagoRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public bool EsEfectivo { get; set; }
    public bool RequiereComprobante { get; set; }
    public bool Activo { get; set; } = true;
}
