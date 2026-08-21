using System.Xml.Serialization;

namespace FacturacionElectronicaDGII.Modelos;

// Acuse de recibo (ARECF) que DGII devuelve sincrónicamente al recibir un e-CF —
// confirmado contra el ejemplo real en Descripcion Tecnica Emisores Electronicos.pdf
// (págs. 10-11). OJO: esto NO es el estado final (Aceptado/Rechazado/Condicional) —
// es solo la confirmación de que el envío llegó; el estado real se consulta después
// por TrackId contra el servicio de consulta de resultado (URL aún no disponible,
// ver ConsultarEstadoAsync).
[XmlRoot("ARECF", Namespace = "")]
public class ArecfDto
{
    [XmlElement("DetalleAcuseDeRecibo")]
    public required DetalleAcuseDeReciboDto DetalleAcuseDeRecibo { get; set; }
}

public class DetalleAcuseDeReciboDto
{
    [XmlElement("Version")]
    public string Version { get; set; } = "1.0";

    [XmlElement("RNCEmisor")]
    public required string RncEmisor { get; set; }

    [XmlElement("RNCComprador")]
    public string? RncComprador { get; set; }

    [XmlElement("eNCF")]
    public required string ENcf { get; set; }

    // 0 = recibido correctamente (formato/firma válidos); otros valores indican rechazo
    // inmediato (ej. XML mal formado, firma inválida) — el catálogo exacto de códigos
    // de rechazo no está documentado en los PDFs disponibles en este proyecto.
    [XmlElement("Estado")]
    public int Estado { get; set; }

    [XmlElement("FechaHoraAcuseDeRecibo")]
    public string? FechaHoraAcuseDeRecibo { get; set; }

    [XmlElement("TrackId")]
    public string? TrackId { get; set; }

    [XmlElement("Mensaje")]
    public string? Mensaje { get; set; }
}
