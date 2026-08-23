namespace SaborByte.Dominio.Sucursales;

public class Sucursal
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Empresa dueña de esta sucursal — obligatorio. Su nombre (no el de la sucursal)
    // es el que se muestra en el AppBar/login de las 4 apps.
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public required string Nombre { get; set; }

    // Código de 2 dígitos único de la sucursal, usado como prefijo del número de
    // factura interno (ver Factura.NumeroFactura): CodigoSucursal + CodigoCaja + secuencia.
    public string? Codigo { get; set; }

    // Contador del número interno de las notas de crédito/débito (NotaCredito.NumeroNota):
    // CodigoSucursal(2) + TipoComprobante(2: "33"/"34") + esta secuencia(5) = 9 dígitos, igual
    // formato que Factura.NumeroFactura. Es por sucursal, no por caja, porque las notas se
    // emiten desde Central sin estar atadas a una caja/turno.
    public long ProximoNumeroNota { get; set; } = 1;

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
    // Nullable: la sucursal de siembra inicial no tiene un usuario autenticado que la haya creado.
    public Guid? CreadoPorUsuarioId { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public Guid? ActualizadoPorUsuarioId { get; set; }
}
