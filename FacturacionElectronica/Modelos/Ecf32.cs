using System.Xml.Serialization;

namespace FacturacionElectronicaDGII.Modelos.Ecf32;

// Modelo generado a partir del XSD oficial de DGII "e-CF 32 v.1.0.xsd" (Comprobante
// Fiscal Electrónico tipo 32 — Factura de Consumo). Los nombres de elementos son
// literales del XSD (no traducir/normalizar). El XSD no declara targetNamespace,
// por lo tanto el XML NO debe llevar namespace (ver [XmlRoot(Namespace = "")]).
// Fechas van como string "dd-MM-yyyy" / "dd-MM-yyyy HH:mm:ss", no xs:date.

[XmlRoot("ECF", Namespace = "")]
public class Ecf32
{
    [XmlElement("Encabezado")]
    public required Encabezado Encabezado { get; set; }

    [XmlArray("DetallesItems")]
    [XmlArrayItem("Item")]
    public List<Item> DetallesItems { get; set; } = [];

    // FechaHoraFirma: fecha/hora del momento de firmar (formato dd-MM-yyyy HH:mm:ss).
    // La firma digital (XAdES) en sí se agrega DESPUÉS de serializar este objeto,
    // el XSD reserva el lugar con <xs:any processContents="skip"/> tras este campo.
    [XmlElement("FechaHoraFirma")]
    public required string FechaHoraFirma { get; set; }
}

public class Encabezado
{
    [XmlElement("Version")]
    public decimal Version { get; set; } = 1.0m;

    [XmlElement("IdDoc")]
    public required IdDoc IdDoc { get; set; }

    [XmlElement("Emisor")]
    public required Emisor Emisor { get; set; }

    [XmlElement("Comprador")]
    public Comprador? Comprador { get; set; }

    [XmlElement("Totales")]
    public required Totales Totales { get; set; }
}

public class IdDoc
{
    // TipoeCF fijo en "32" para Comprobante de Consumo Electrónico.
    [XmlElement("TipoeCF")]
    public string TipoeCF { get; set; } = "32";

    // eNCF: 13 caracteres alfanuméricos (pattern XSD: [a-z0-9A-Z]{13}).
    [XmlElement("eNCF")]
    public required string ENCF { get; set; }

    // TipoIngresos: 01-06 según catálogo DGII (01 Ingresos por operaciones... ver doc oficial).
    [XmlElement("TipoIngresos")]
    public required string TipoIngresos { get; set; }

    // TipoPago: 1 Contado, 2 Crédito, 3 Gratuito.
    [XmlElement("TipoPago")]
    public int TipoPago { get; set; } = 1;

    [XmlArray("TablaFormasPago")]
    [XmlArrayItem("FormaDePago")]
    public List<FormaDePago> TablaFormasPago { get; set; } = [];
}

public class FormaDePago
{
    // FormaPago: 1 Efectivo, 2 Cheque/Transferencia/Depósito, 3 Tarjeta Débito/Crédito,
    // 4 Venta a Crédito, 5 Bonos o Certificados de Regalo, 6 Permuta, 7 Nota de Crédito, 8 Otras.
    [XmlElement("FormaPago")]
    public int FormaPago { get; set; }

    [XmlElement("MontoPago")]
    public decimal MontoPago { get; set; }
}

public class Emisor
{
    [XmlElement("RNCEmisor")]
    public required string RNCEmisor { get; set; }

    [XmlElement("RazonSocialEmisor")]
    public required string RazonSocialEmisor { get; set; }

    [XmlElement("NombreComercial")]
    public string? NombreComercial { get; set; }

    [XmlElement("DireccionEmisor")]
    public required string DireccionEmisor { get; set; }

    // FechaEmision: formato dd-MM-yyyy.
    [XmlElement("FechaEmision")]
    public required string FechaEmision { get; set; }
}

public class Comprador
{
    [XmlElement("RNCComprador")]
    public string? RNCComprador { get; set; }

    [XmlElement("RazonSocialComprador")]
    public string? RazonSocialComprador { get; set; }
}

public class Totales
{
    [XmlElement("MontoGravadoTotal")]
    public decimal? MontoGravadoTotal { get; set; }

    [XmlElement("MontoExento")]
    public decimal? MontoExento { get; set; }

    // ITBIS1: tasa en % (18, 16 o 0), no el monto.
    [XmlElement("ITBIS1")]
    public int? ITBIS1 { get; set; }

    [XmlElement("TotalITBIS")]
    public decimal? TotalITBIS { get; set; }

    [XmlElement("MontoTotal")]
    public decimal MontoTotal { get; set; }
}

public class Item
{
    [XmlElement("NumeroLinea")]
    public int NumeroLinea { get; set; }

    // IndicadorFacturacion: 1 ITBIS1 18%, 2 ITBIS2 16%, 3 ITBIS3 0%, 4 Exento, 0 No facturable.
    [XmlElement("IndicadorFacturacion")]
    public int IndicadorFacturacion { get; set; }

    [XmlElement("NombreItem")]
    public required string NombreItem { get; set; }

    // IndicadorBienoServicio: 1 Bien, 2 Servicio.
    [XmlElement("IndicadorBienoServicio")]
    public int IndicadorBienoServicio { get; set; } = 2; // restaurante: normalmente Servicio

    [XmlElement("CantidadItem")]
    public decimal CantidadItem { get; set; }

    [XmlElement("PrecioUnitarioItem")]
    public decimal PrecioUnitarioItem { get; set; }

    [XmlElement("DescuentoMonto")]
    public decimal? DescuentoMonto { get; set; }

    [XmlElement("MontoItem")]
    public decimal MontoItem { get; set; }
}
