namespace SaborByte.Dominio.Sucursales;

public class Sucursal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Rnc { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; } = true;

    public bool ModuloMeseroActivo { get; set; }
    public bool ModuloCocinaActivo { get; set; }
    public bool EcfActivo { get; set; }
    public bool SmtpActivo { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
