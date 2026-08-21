namespace FacturacionElectronicaDGII;

public class FacturacionElectronicaOpciones
{
    // Ruta al certificado digital (.pfx/.p12) homologado ante DGII y su contraseña.
    // En desarrollo van en appsettings.Development.json; en producción deben venir
    // de un vault de secretos, nunca en texto plano en el repositorio.
    public string? RutaCertificado { get; set; }
    public string? PasswordCertificado { get; set; }

    // URLs de los servicios web REST de DGII. Los valores por defecto son los del
    // ambiente de pruebas "TesteCF" (confirmados por el usuario, no inventados) —
    // para producción, DGII entrega un set de URLs equivalente que hay que sobreescribir
    // vía configuración (appsettings/vault), NUNCA hardcodear un cambio de host aquí.
    public string UrlSemilla { get; set; } = "https://ecf.dgii.gov.do/testecf/autenticacion/api/Autenticacion/Semilla";
    public string UrlValidacionCertificado { get; set; } = "https://ecf.dgii.gov.do/testecf/autenticacion/api/Autenticacion/ValidarSemilla";
    public string UrlRecepcionEcf { get; set; } = "https://ecf.dgii.gov.do/testecf/recepcion/api/FacturasElectronicas";

    // ConsultaTrackId espera el TrackId concatenado como query string (?TrackId=...),
    // no como parámetro separado — así lo entregó DGII.
    public string UrlConsultaTrackId { get; set; } = "https://ecf.dgii.gov.do/testecf/consultaresultado/api/Consultas/Estado?TrackId=";
}
