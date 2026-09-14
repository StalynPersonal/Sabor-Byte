using System.Xml;
using FacturacionElectronicaDGII.Firma;

namespace FacturacionElectronicaDGII.Tests;

public class FirmadorXmlTests
{
    private const string XmlDeEjemplo = "<ECF><Contenido>algo que firmar</Contenido></ECF>";

    [Fact]
    public void Firma_el_xml_y_agrega_un_nodo_Signature_con_estructura_xmldsig()
    {
        var certificado = CertificadoDePrueba.GenerarConClavePrivada();

        var xmlFirmado = FirmadorXml.FirmarEnveloped(XmlDeEjemplo, certificado);

        var documento = new XmlDocument();
        documento.LoadXml(xmlFirmado);
        var gestor = new XmlNamespaceManager(documento.NameTable);
        gestor.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

        Assert.NotNull(documento.SelectSingleNode("//ds:Signature", gestor));
        Assert.NotNull(documento.SelectSingleNode("//ds:Signature/ds:SignedInfo", gestor));
        Assert.NotNull(documento.SelectSingleNode("//ds:Signature/ds:SignatureValue", gestor));
        Assert.NotNull(documento.SelectSingleNode("//ds:Signature/ds:KeyInfo/ds:X509Data/ds:X509Certificate", gestor));

        // El contenido original del comprobante sigue intacto (firma "enveloped").
        Assert.Contains("<Contenido>algo que firmar</Contenido>", xmlFirmado);
    }

    [Fact]
    public void El_signature_method_es_rsa_sha256_segun_lo_que_dgii_acepta()
    {
        var certificado = CertificadoDePrueba.GenerarConClavePrivada();

        var xmlFirmado = FirmadorXml.FirmarEnveloped(XmlDeEjemplo, certificado);

        Assert.Contains("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", xmlFirmado);
        Assert.Contains("http://www.w3.org/2001/04/xmlenc#sha256", xmlFirmado);
    }

    [Fact]
    public void Falla_si_el_certificado_no_tiene_clave_privada()
    {
        var certificadoSoloPublico = CertificadoDePrueba.GenerarSoloPublico();

        var excepcion = Assert.Throws<InvalidOperationException>(
            () => FirmadorXml.FirmarEnveloped(XmlDeEjemplo, certificadoSoloPublico));

        Assert.Contains("clave privada", excepcion.Message);
    }

    [Fact]
    public void El_codigo_de_seguridad_se_puede_derivar_del_xml_ya_firmado()
    {
        // Prueba de integración entre las dos piezas: firmar y luego extraer el código de
        // seguridad son pasos consecutivos del flujo real (ver ServicioFacturacionElectronicaDgii).
        var certificado = CertificadoDePrueba.GenerarConClavePrivada();
        var xmlFirmado = FirmadorXml.FirmarEnveloped(XmlDeEjemplo, certificado);

        var codigo = GeneradorCodigoSeguridad.Generar(xmlFirmado);

        Assert.Equal(6, codigo.Length);
    }
}
