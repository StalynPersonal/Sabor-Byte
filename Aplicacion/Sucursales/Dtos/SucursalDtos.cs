namespace SaborByte.Aplicacion.Sucursales.Dtos;

// Singleton: solo existe una fila de Empresa en todo el sistema (multisucursal, no
// multiempresa). No hay Crear/Listar — solo Obtener/Actualizar la única fila sembrada.
public class EmpresaDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public string? Rnc { get; set; }
}

public class GuardarEmpresaRequestDto
{
    public required string Nombre { get; set; }
    public string? Rnc { get; set; }
}

public class SucursalResumenDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public string? Codigo { get; set; }
    public bool Activa { get; set; }
}

public class CrearSucursalRequestDto
{
    public required string Nombre { get; set; }
    public required string Codigo { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool ModuloMeseroActivo { get; set; }
    public bool ModuloCocinaActivo { get; set; }
    public bool EcfActivo { get; set; }
}

public class SucursalDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public string? Codigo { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; }
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
    public required string Nombre { get; set; }
    public string? Codigo { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; } = true;
    public bool ModuloMeseroActivo { get; set; }
    public bool ModuloCocinaActivo { get; set; }
    public bool EcfActivo { get; set; }
}

public class ActualizarSmtpRequestDto
{
    public bool SmtpActivo { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPuerto { get; set; }
    public string? SmtpUsuario { get; set; }
    public string? SmtpPassword { get; set; } // si viene null, se conserva la contraseña actual
    public string? SmtpRemitente { get; set; }
    public bool SmtpUsaSsl { get; set; } = true;
}

public class EnviarPruebaSmtpRequestDto
{
    public required string Destinatario { get; set; }
}
