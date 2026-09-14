using System.Xml;
using FacturacionElectronicaDGII.Modelos;

namespace FacturacionElectronicaDGII.Tests;

public class GeneradorXmlEcf32Tests
{
    private static ComprobanteDto ComprobanteDeEjemplo(decimal tasaItbis = 0.18m) => new()
    {
        TipoNcf = "32",
        NumeroNcf = "E320001170280",
        FechaEmision = new DateTime(2026, 3, 5),
        Emisor = new EmisorDto { Rnc = "130123456", RazonSocial = "SaborByte SRL" },
        Detalle =
        [
            new LineaComprobanteDto { Descripcion = "Pastel de chocolate", Cantidad = 2, PrecioUnitario = 100m, TasaItbis = tasaItbis, Impuesto = 36m, Total = 236m }
        ],
        Subtotal = 200m,
        MontoImpuestos = 36m,
        Total = 236m
    };

    [Fact]
    public void Genera_xml_bien_formado_y_parseable()
    {
        var xml = GeneradorXmlEcf32.Generar(ComprobanteDeEjemplo());

        var documento = new XmlDocument();
        var excepcion = Record.Exception(() => documento.LoadXml(xml));

        Assert.Null(excepcion);
    }

    [Fact]
    public void Incluye_los_datos_del_emisor_y_el_encf()
    {
        var xml = GeneradorXmlEcf32.Generar(ComprobanteDeEjemplo());

        Assert.Contains("<eNCF>E320001170280</eNCF>", xml);
        Assert.Contains("<RNCEmisor>130123456</RNCEmisor>", xml);
        Assert.Contains("<RazonSocialEmisor>SaborByte SRL</RazonSocialEmisor>", xml);
        Assert.Contains("<FechaEmision>05-03-2026</FechaEmision>", xml);
    }

    [Fact]
    public void No_incluye_nodo_comprador_cuando_no_hay_comprador()
    {
        var comprobante = ComprobanteDeEjemplo();
        comprobante.Comprador = null;

        var xml = GeneradorXmlEcf32.Generar(comprobante);

        Assert.DoesNotContain("<Comprador>", xml);
    }

    [Fact]
    public void Incluye_comprador_cuando_esta_presente()
    {
        var comprobante = ComprobanteDeEjemplo();
        comprobante.Comprador = new CompradorDto { RncOCedula = "101234567", NombreORazonSocial = "Cliente Fiscal SRL" };

        var xml = GeneradorXmlEcf32.Generar(comprobante);

        Assert.Contains("<RNCComprador>101234567</RNCComprador>", xml);
        Assert.Contains("<RazonSocialComprador>Cliente Fiscal SRL</RazonSocialComprador>", xml);
    }

    [Fact]
    public void Incluye_los_totales_del_comprobante()
    {
        var xml = GeneradorXmlEcf32.Generar(ComprobanteDeEjemplo());

        Assert.Contains("<MontoGravadoTotal>200</MontoGravadoTotal>", xml);
        Assert.Contains("<TotalITBIS>36</TotalITBIS>", xml);
        Assert.Contains("<MontoTotal>236</MontoTotal>", xml);
    }

    [Theory]
    [InlineData(0.18, 1)]
    [InlineData(0.16, 2)]
    [InlineData(0.0, 3)]
    public void Traduce_la_tasa_de_itbis_al_indicador_de_facturacion_del_catalogo_dgii(double tasa, int indicadorEsperado)
    {
        var xml = GeneradorXmlEcf32.Generar(ComprobanteDeEjemplo((decimal)tasa));

        Assert.Contains($"<IndicadorFacturacion>{indicadorEsperado}</IndicadorFacturacion>", xml);
    }

    [Fact]
    public void Tasa_de_itbis_no_soportada_lanza_excepcion_clara()
    {
        var comprobante = ComprobanteDeEjemplo(0.05m); // tasa inexistente en el catálogo DGII

        var excepcion = Assert.Throws<NotSupportedException>(() => GeneradorXmlEcf32.Generar(comprobante));
        Assert.Contains("IndicadorFacturacion", excepcion.Message);
    }

    [Fact]
    public void Numera_las_lineas_de_detalle_consecutivamente_desde_uno()
    {
        var comprobante = ComprobanteDeEjemplo();
        comprobante.Detalle.Add(new LineaComprobanteDto { Descripcion = "Refresco", Cantidad = 1, PrecioUnitario = 50m, TasaItbis = 0.18m, Impuesto = 9m, Total = 59m });

        var xml = GeneradorXmlEcf32.Generar(comprobante);

        Assert.Contains("<NumeroLinea>1</NumeroLinea>", xml);
        Assert.Contains("<NumeroLinea>2</NumeroLinea>", xml);
    }
}
