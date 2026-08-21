using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace FacturacionElectronicaDGII.Firma;

// Firma XML-DSig "enveloped signature" verificada contra un e-CF 32 real y firmado
// provisto por el usuario (carpeta /xsd): confirma que DGII acepta XML-DSig estándar
// (NO exige el bloque XAdES con propiedades calificadoras) — SignatureMethod
// rsa-sha256, DigestMethod sha256, CanonicalizationMethod C14N (no exclusivo),
// Reference URI="" con transform enveloped-signature, y KeyInfo con el certificado
// X.509 completo embebido. Esta implementación reproduce esa misma estructura.
public static class FirmadorXml
{
    private const string SignatureMethodRsaSha256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
    private const string DigestMethodSha256 = "http://www.w3.org/2001/04/xmlenc#sha256";

    public static string FirmarEnveloped(string xmlSinFirmar, X509Certificate2 certificado)
    {
        if (!certificado.HasPrivateKey)
            throw new InvalidOperationException("El certificado no tiene clave privada; no se puede firmar.");

        var documento = new XmlDocument { PreserveWhitespace = true };
        documento.LoadXml(xmlSinFirmar);

        var firmadoXml = new SignedXml(documento)
        {
            SigningKey = certificado.GetRSAPrivateKey()
        };
        firmadoXml.SignedInfo!.SignatureMethod = SignatureMethodRsaSha256;

        var referencia = new Reference { Uri = "", DigestMethod = DigestMethodSha256 };
        referencia.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        referencia.AddTransform(new XmlDsigC14NTransform());
        firmadoXml.AddReference(referencia);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificado));
        firmadoXml.KeyInfo = keyInfo;

        firmadoXml.ComputeSignature();

        var nodoFirma = firmadoXml.GetXml();
        documento.DocumentElement!.AppendChild(documento.ImportNode(nodoFirma, deep: true));

        return documento.OuterXml;
    }
}
