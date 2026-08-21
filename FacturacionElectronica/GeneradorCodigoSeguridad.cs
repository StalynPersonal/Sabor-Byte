using System.Xml;

namespace FacturacionElectronicaDGII;

// Confirmado contra la documentación oficial de DGII (Informe Técnico e-CF v1.0,
// sección 18.2.3, y el glosario del Instructivo del Facturador Gratuito de FE):
// "CodigoSeguridad: corresponde a los primeros seis (6) dígitos del hash generado
// en el SignatureValue de la firma digital del e-CF". No es un hash propio calculado
// sobre campos del comprobante (como se asumía en la versión anterior de este archivo)
// — es directamente el prefijo del valor Base64 ya presente en <SignatureValue> de la
// firma XML-DSig. Confirmado también empíricamente: ningún e-CF real de ejemplo
// (carpeta /xsd) trae un campo CodigoSeguridadeCF dentro del XML firmado — el código
// se usa solo externamente, en la URL de consulta de comprobante (QR / representación
// impresa), nunca dentro del documento firmado.
public static class GeneradorCodigoSeguridad
{
    public static string Generar(string comprobanteFirmado)
    {
        var documento = new XmlDocument();
        documento.LoadXml(comprobanteFirmado);

        var gestorNamespaces = new XmlNamespaceManager(documento.NameTable);
        gestorNamespaces.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

        var nodoValorFirma = documento.SelectSingleNode("//ds:Signature/ds:SignatureValue", gestorNamespaces)
            ?? throw new InvalidOperationException(
                "El XML no contiene un elemento SignatureValue; ¿el comprobante fue firmado antes de generar el código de seguridad?");

        var valorFirma = nodoValorFirma.InnerText.Trim();
        if (valorFirma.Length < 6)
            throw new InvalidOperationException("El SignatureValue es demasiado corto para derivar el código de seguridad.");

        return valorFirma[..6];
    }
}
