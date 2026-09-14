namespace FacturacionElectronicaDGII.Tests;

public class GeneradorCodigoSeguridadTests
{
    private const string XmlConFirma =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <ECF>
          <Contenido>algo</Contenido>
          <ds:Signature xmlns:ds="http://www.w3.org/2000/09/xmldsig#">
            <ds:SignedInfo></ds:SignedInfo>
            <ds:SignatureValue>ABCDEF1234567890==</ds:SignatureValue>
          </ds:Signature>
        </ECF>
        """;

    [Fact]
    public void Devuelve_los_primeros_seis_caracteres_del_SignatureValue()
    {
        var codigo = GeneradorCodigoSeguridad.Generar(XmlConFirma);

        Assert.Equal("ABCDEF", codigo);
    }

    [Fact]
    public void Falla_si_el_xml_no_tiene_nodo_SignatureValue()
    {
        const string xmlSinFirma = "<ECF><Contenido>algo</Contenido></ECF>";

        var excepcion = Assert.Throws<InvalidOperationException>(() => GeneradorCodigoSeguridad.Generar(xmlSinFirma));
        Assert.Contains("SignatureValue", excepcion.Message);
    }

    [Fact]
    public void Falla_si_el_SignatureValue_es_demasiado_corto()
    {
        const string xmlFirmaCorta =
            """
            <ECF>
              <ds:Signature xmlns:ds="http://www.w3.org/2000/09/xmldsig#">
                <ds:SignatureValue>abc</ds:SignatureValue>
              </ds:Signature>
            </ECF>
            """;

        var excepcion = Assert.Throws<InvalidOperationException>(() => GeneradorCodigoSeguridad.Generar(xmlFirmaCorta));
        Assert.Contains("demasiado corto", excepcion.Message);
    }
}
