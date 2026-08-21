namespace SaborByte.Web.Api.Dtos;

public class SucursalDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Rnc { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool ModuloMeseroActivo { get; set; }
    public bool ModuloCocinaActivo { get; set; }
    public bool EcfActivo { get; set; }
    public bool SmtpActivo { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPuerto { get; set; }
    public string? SmtpUsuario { get; set; }
    public string? SmtpRemitente { get; set; }
    public bool SmtpUsaSsl { get; set; }
}

public class ActualizarSucursalRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Rnc { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool ModuloMeseroActivo { get; set; }
    public bool ModuloCocinaActivo { get; set; }
    public bool EcfActivo { get; set; }
    public bool SmtpActivo { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPuerto { get; set; }
    public string? SmtpUsuario { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpRemitente { get; set; }
    public bool SmtpUsaSsl { get; set; } = true;
}
