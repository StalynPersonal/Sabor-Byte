using FacturacionElectronicaDGII.Modelos;
using FacturacionElectronicaDGII.Validacion;

namespace FacturacionElectronicaDGII.Tests;

public class ValidadorComprobanteTests
{
    private static ComprobanteDto ComprobanteValido() => new()
    {
        TipoNcf = "32",
        NumeroNcf = "E320001170280",
        FechaEmision = DateTime.UtcNow,
        Emisor = new EmisorDto { Rnc = "130123456", RazonSocial = "SaborByte SRL" },
        Detalle =
        [
            new LineaComprobanteDto { Descripcion = "Pastel de chocolate", Cantidad = 1, PrecioUnitario = 100m, TasaItbis = 0.18m, Impuesto = 18m, Total = 118m }
        ],
        Subtotal = 100m,
        MontoImpuestos = 18m,
        Total = 118m
    };

    [Fact]
    public void Comprobante_valido_no_produce_errores()
    {
        var resultado = ValidadorComprobante.Validar(ComprobanteValido());

        Assert.True(resultado.EsValido);
        Assert.Empty(resultado.Errores);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")] // ni 9 ni 11 dígitos
    [InlineData("12345678A")] // no numérico
    public void Rnc_emisor_invalido_o_vacio_produce_error(string rncInvalido)
    {
        var comprobante = ComprobanteValido();
        comprobante.Emisor.Rnc = rncInvalido;

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.Contains("RNC del emisor"));
    }

    [Fact]
    public void Razon_social_emisor_vacia_produce_error()
    {
        var comprobante = ComprobanteValido();
        comprobante.Emisor.RazonSocial = "  ";

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains("razón social del emisor"));
    }

    [Fact]
    public void Secuencia_vencida_antes_de_la_emision_produce_error()
    {
        var comprobante = ComprobanteValido();
        comprobante.FechaEmision = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        comprobante.FechaVencimientoSecuencia = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains("secuencia de NCF/e-CF está vencida"));
    }

    [Theory]
    [InlineData("31")] // Crédito Fiscal
    [InlineData("45")] // Gubernamental
    public void Tipos_que_requieren_comprador_sin_rnc_de_comprador_producen_error(string tipoNcf)
    {
        var comprobante = ComprobanteValido();
        comprobante.TipoNcf = tipoNcf;
        comprobante.Comprador = null;

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains($"tipo de comprobante {tipoNcf}"));
    }

    [Fact]
    public void Tipo_32_no_requiere_comprador()
    {
        var comprobante = ComprobanteValido(); // TipoNcf = "32", sin Comprador

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.DoesNotContain(resultado.Errores, e => e.Contains("RNC/Cédula registrado"));
    }

    [Fact]
    public void Comprador_con_rnc_invalido_produce_error_aunque_el_tipo_lo_requiera()
    {
        var comprobante = ComprobanteValido();
        comprobante.TipoNcf = "31";
        comprobante.Comprador = new CompradorDto { RncOCedula = "123", NombreORazonSocial = "Cliente Fiscal" };

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains("RNC/Cédula del comprador"));
    }

    [Fact]
    public void Fecha_emision_futura_produce_error()
    {
        var comprobante = ComprobanteValido();
        comprobante.FechaEmision = DateTime.UtcNow.AddDays(5);

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains("no puede ser futura"));
    }

    [Fact]
    public void Sin_lineas_de_detalle_produce_error()
    {
        var comprobante = ComprobanteValido();
        comprobante.Detalle = [];

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains("al menos una línea de detalle"));
    }

    [Fact]
    public void Total_que_no_coincide_con_subtotal_mas_impuestos_produce_error()
    {
        var comprobante = ComprobanteValido();
        comprobante.Total = 999m;

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains("no coincide con subtotal"));
    }

    [Fact]
    public void Linea_con_cantidad_cero_y_precio_negativo_produce_dos_errores()
    {
        var comprobante = ComprobanteValido();
        comprobante.Detalle[0].Cantidad = 0;
        comprobante.Detalle[0].PrecioUnitario = -5;

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains("cantidad mayor a cero"));
        Assert.Contains(resultado.Errores, e => e.Contains("precio unitario negativo"));
    }

    [Fact]
    public void Tipo_32_con_mas_de_1000_lineas_y_total_mayor_o_igual_a_250000_no_produce_error_de_maximo()
    {
        var comprobante = ComprobanteValido();
        comprobante.TipoNcf = "32";
        comprobante.Total = 300_000m;
        comprobante.Subtotal = 300_000m - 45_000m;
        comprobante.MontoImpuestos = 45_000m;
        comprobante.Detalle = Enumerable.Range(1, 1000)
            .Select(i => new LineaComprobanteDto { Descripcion = $"Item {i}", Cantidad = 1, PrecioUnitario = 255m, TasaItbis = 0.18m, Impuesto = 45.9m, Total = 300.9m })
            .ToList();

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.DoesNotContain(resultado.Errores, e => e.Contains("supera el máximo permitido"));
    }

    [Fact]
    public void Tipo_32_con_mas_de_1000_lineas_y_total_mayor_o_igual_a_250000_produce_error_de_maximo()
    {
        // El límite es más estricto (1,000) cuando el total es GRANDE (≥ 250,000) — con
        // total menor, el límite sube a 10,000 (ver ValidarMaximoDeLineas).
        var comprobante = ComprobanteValido();
        comprobante.TipoNcf = "32";
        comprobante.Total = 300_000m;
        comprobante.Subtotal = 300_000m - 45_000m;
        comprobante.MontoImpuestos = 45_000m;
        comprobante.Detalle = Enumerable.Range(1, 1001)
            .Select(i => new LineaComprobanteDto { Descripcion = $"Item {i}", Cantidad = 1, PrecioUnitario = 255m, TasaItbis = 0.18m, Impuesto = 45.9m, Total = 300.9m })
            .ToList();

        var resultado = ValidadorComprobante.Validar(comprobante);

        Assert.Contains(resultado.Errores, e => e.Contains("supera el máximo permitido (1000)"));
    }
}
