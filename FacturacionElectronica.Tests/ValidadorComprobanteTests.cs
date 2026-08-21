using FacturacionElectronicaDGII.Modelos;
using FacturacionElectronicaDGII.Validacion;
using Xunit;

namespace FacturacionElectronicaDGII.Tests;

public class ValidadorComprobanteTests
{
    private static ComprobanteDto ComprobanteValido() => new()
    {
        TipoNcf = "32",
        NumeroNcf = "E320000000123",
        FechaEmision = DateTime.UtcNow,
        Emisor = new EmisorDto { Rnc = "130123456", RazonSocial = "Sabor Byte SRL" },
        Detalle = [new LineaComprobanteDto { Descripcion = "Producto", Cantidad = 1, PrecioUnitario = 100m, Total = 118m }],
        Subtotal = 100m,
        MontoImpuestos = 18m,
        Total = 118m
    };

    [Fact]
    public void Validar_ComprobanteCorrecto_EsValido()
    {
        var resultado = ValidadorComprobante.Validar(ComprobanteValido());
        Assert.True(resultado.EsValido);
    }

    [Fact]
    public void Validar_MontoTotalCero_EsInvalido()
    {
        var comprobante = ComprobanteValido();
        comprobante.Total = 0;

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.Contains("total"));
    }

    [Fact]
    public void Validar_RncEmisorInvalido_EsInvalido()
    {
        var comprobante = ComprobanteValido();
        comprobante.Emisor.Rnc = "123";

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.False(resultado.EsValido);
    }

    [Fact]
    public void Validar_TipoCreditoFiscalSinCompradorConRnc_EsInvalido()
    {
        var comprobante = ComprobanteValido();
        comprobante.TipoNcf = "E31";
        comprobante.Comprador = null;

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.False(resultado.EsValido);
    }

    [Fact]
    public void Validar_TotalNoCoincideConSubtotalMasImpuestos_EsInvalido()
    {
        var comprobante = ComprobanteValido();
        comprobante.Total = 999m;

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.False(resultado.EsValido);
    }
}
