namespace SaborByte.Aplicacion.Catalogo.Dtos;

public class MetodoPagoDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public bool EsEfectivo { get; set; }
    public bool RequiereComprobante { get; set; }
    public bool Activo { get; set; }
}

public class GuardarMetodoPagoRequestDto
{
    public required string Nombre { get; set; }
    public bool EsEfectivo { get; set; }
    public bool RequiereComprobante { get; set; }
    public bool Activo { get; set; } = true;
}
