using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace FacturacionElectronicaDGII.Firma;

// ADVERTENCIA: esto firma el XML con XML-DSig "enveloped signature" estándar de .NET
// (System.Security.Cryptography.Xml), que es un punto de partida técnicamente correcto,
// pero DGII exige específicamente XAdES-BES (ETSI TS 101 903), que añade propiedades
// calificadoras (SigningTime, SigningCertificate, etc.) sobre el XML-DSig base. Antes
// de certificar contra DGII, verificar si esta firma "plana" es aceptada o si hace
// falta añadir el bloque <xades:QualifyingProperties> (no cubierto aquí porque su
// estructura exacta no está en los XSD disponibles en este proyecto).
public static class FirmadorXml
{
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

        var referencia = new Reference { Uri = "" };
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
