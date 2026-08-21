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

    // Configuración de correo saliente, opcional por sucursal (ver sección "Configuración"
    // del plan). Si SmtpActivo = false, IEmailSender no envía nada silenciosamente.
    public bool SmtpActivo { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPuerto { get; set; }
    public string? SmtpUsuario { get; set; }
    public string? SmtpPassword { get; set; } // en producción: resguardar en vault, no en texto plano
    public string? SmtpRemitente { get; set; }
    public bool SmtpUsaSsl { get; set; } = true;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
