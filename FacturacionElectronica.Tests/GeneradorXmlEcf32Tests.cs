using System.Xml.Linq;
using FacturacionElectronicaDGII;
using FacturacionElectronicaDGII.Modelos;
using Xunit;

namespace FacturacionElectronicaDGII.Tests;

public class GeneradorXmlEcf32Tests
{
    private static ComprobanteDto ComprobanteDeEjemplo() => new()
    {
        TipoNcf = "32",
        NumeroNcf = "E320000000123",
        FechaEmision = new DateTime(2026, 8, 21),
        Emisor = new EmisorDto { Rnc = "130123456", RazonSocial = "Sabor Byte SRL" },
        Comprador = new CompradorDto { RncOCedula = "40212345678", NombreORazonSocial = "Cliente de Prueba" },
        Detalle =
        [
            new LineaComprobanteDto { Descripcion = "Hamburguesa Clasica", Cantidad = 2, PrecioUnitario = 250m, Impuesto = 90m, Total = 590m }
        ],
        Subtotal = 500m,
        MontoImpuestos = 90m,
        Total = 590m
    };

    [Fact]
    public void Generar_ProduceXmlSinNamespace()
    {
        var xml = GeneradorXmlEcf32.Generar(ComprobanteDeEjemplo());
        var doc = XDocument.Parse(xml);

        Assert.Equal("ECF", doc.Root!.Name.LocalName);
        Assert.Equal(string.Empty, doc.Root.Name.NamespaceName);
    }

    [Fact]
    public void Generar_IncluyeCamposObligatoriosDelEncabezado()
    {
        var xml = GeneradorXmlEcf32.Generar(ComprobanteDeEjemplo());
        var doc = XDocument.Parse(xml);

        var idDoc = doc.Root!.Element("Encabezado")!.Element("IdDoc")!;
        Assert.Equal("32", idDoc.Element("TipoeCF")!.Value);
        Assert.Equal("E320000000123", idDoc.Element("eNCF")!.Value);

        var emisor = doc.Root.Element("Encabezado")!.Element("Emisor")!;
        Assert.Equal("130123456", emisor.Element("RNCEmisor")!.Value);
        Assert.Equal("21-08-2026", emisor.Element("FechaEmision")!.Value); // formato dd-MM-yyyy exigido por DGII

        var totales = doc.Root.Element("Encabezado")!.Element("Totales")!;
        // El XSD permite decimal con hasta 2 posiciones opcionales ([0-9]{1,16}(\.[0-9]{1,2})?);
        // XmlSerializer omite el ".00" cuando no hay parte decimal, lo cual sigue siendo válido.
        Assert.Equal(590m, decimal.Parse(totales.Element("MontoTotal")!.Value));
    }

    [Fact]
    public void Generar_IncluyeUnaLineaPorItemDelDetalle()
    {
        var xml = GeneradorXmlEcf32.Generar(ComprobanteDeEjemplo());
        var doc = XDocument.Parse(xml);

        var items = doc.Root!.Element("DetallesItems")!.Elements("Item").ToList();
        Assert.Single(items);
        Assert.Equal("Hamburguesa Clasica", items[0].Element("NombreItem")!.Value);
        Assert.Equal("1", items[0].Element("NumeroLinea")!.Value);
    }

    [Fact]
    public void Generar_RechazaTiposDistintosA32()
    {
        var comprobante = ComprobanteDeEjemplo();
        comprobante.TipoNcf = "31";

        Assert.Throws<NotSupportedException>(() =>
            new ServicioFacturacionElectronicaDgii(new FacturacionElectronicaOpciones()).GenerarComprobanteXml(comprobante));
    }
}
