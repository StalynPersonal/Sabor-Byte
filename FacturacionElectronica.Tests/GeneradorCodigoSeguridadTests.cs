using Xunit;

namespace FacturacionElectronicaDGII.Tests;

public class GeneradorCodigoSeguridadTests
{
    private const string XmlFirmadoDeEjemplo = """
        <ECF>
          <Encabezado><Version>1.0</Version></Encabezado>
          <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
            <SignedInfo></SignedInfo>
            <SignatureValue>XOfODhqOHZ5MVmtaBd8+h5WI8wnKa54oqTso1l1Rok9bALfKRZm4ali1OTCzepF</SignatureValue>
          </Signature>
        </ECF>
        """;

    [Fact]
    public void Generar_DevuelveSeisCaracteres()
    {
        var codigo = GeneradorCodigoSeguridad.Generar(XmlFirmadoDeEjemplo);

        Assert.Equal(6, codigo.Length);
    }

    [Fact]
    public void Generar_TomaLosPrimerosSeisCaracteresDelSignatureValue()
    {
        var codigo = GeneradorCodigoSeguridad.Generar(XmlFirmadoDeEjemplo);

        Assert.Equal("XOfODh", codigo);
    }

    [Fact]
    public void Generar_SinSignatureValue_Falla()
    {
        const string xmlSinFirmar = "<ECF><Encabezado><Version>1.0</Version></Encabezado></ECF>";

        Assert.Throws<InvalidOperationException>(() => GeneradorCodigoSeguridad.Generar(xmlSinFirmar));
    }
}
