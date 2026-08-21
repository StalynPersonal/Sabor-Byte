namespace FacturacionElectronicaDGII;

public class FacturacionElectronicaOpciones
{
    // Ruta al certificado digital (.pfx/.p12) homologado ante DGII y su contraseña.
    // En desarrollo van en appsettings.Development.json; en producción deben venir
    // de un vault de secretos, nunca en texto plano en el repositorio.
    public string? RutaCertificado { get; set; }
    public string? PasswordCertificado { get; set; }

    // URL base del ambiente de pruebas/certificación o producción de DGII.
    // No se fija ningún valor por defecto: DGII exige registrar el ambiente exacto
    // y este proyecto no tiene acceso verificado a esas URLs oficiales.
    public string? UrlBaseDgii { get; set; }
}
