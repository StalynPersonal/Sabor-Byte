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
        comprobante.TipoNcf = "31";
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

    // Límite confirmado en "Formato Comprobante Fiscal Electrónico (e-CF) V1.0" (DGII):
    // 100 líneas por defecto para la mayoría de tipos de e-CF.
    [Fact]
    public void Validar_MasDeCienLineasEnTipoDistintoA32_EsInvalido()
    {
        var comprobante = ComprobanteValido();
        comprobante.TipoNcf = "34"; // Nota de Crédito — no tiene la excepción de e-CF 32
        comprobante.Detalle = Enumerable.Range(1, 101)
            .Select(i => new LineaComprobanteDto { Descripcion = $"Item {i}", Cantidad = 1, PrecioUnitario = 1m, Total = 1m })
            .ToList();

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.Contains("máximo permitido"));
    }

    [Fact]
    public void Validar_HastaCienLineasEnTipoDistintoA32_EsValidoEnEseAspecto()
    {
        var comprobante = ComprobanteValido();
        comprobante.TipoNcf = "34";
        comprobante.Detalle = Enumerable.Range(1, 100)
            .Select(i => new LineaComprobanteDto { Descripcion = $"Item {i}", Cantidad = 1, PrecioUnitario = 1m, Total = 1m })
            .ToList();
        comprobante.Subtotal = 100m;
        comprobante.MontoImpuestos = 0m;
        comprobante.Total = 100m;

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.DoesNotContain(resultado.Errores, e => e.Contains("máximo permitido"));
    }
}
