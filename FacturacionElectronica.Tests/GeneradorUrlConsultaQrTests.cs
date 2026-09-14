namespace FacturacionElectronicaDGII.Tests;

public class GeneradorUrlConsultaQrTests
{
    private static readonly FacturacionElectronicaOpciones Opciones = new();

    [Fact]
    public void Sin_rnc_comprador_usa_la_variante_simplificada_ConsultaTimbreFC()
    {
        var url = GeneradorUrlConsultaQr.Construir(
            Opciones,
            rncEmisor: "130123456",
            rncComprador: null,
            numeroNcf: "E320001170280",
            fechaEmision: new DateTime(2026, 3, 5),
            montoTotal: 1180.50m,
            fechaFirma: new DateTime(2026, 3, 5, 14, 30, 0),
            codigoSeguridad: "ABC123");

        Assert.StartsWith(Opciones.UrlConsultaTimbreFc, url);
        Assert.Contains("RNCEmisor=130123456", url);
        Assert.Contains("ENCF=E320001170280", url);
        Assert.Contains("MontoTotal=1180.50", url);
        Assert.Contains("CodigoSeguridad=ABC123", url);
        // La variante simplificada no lleva RncComprador ni fechas.
        Assert.DoesNotContain("RncComprador", url);
        Assert.DoesNotContain("FechaEmision", url);
        Assert.DoesNotContain("FechaFirma", url);
    }

    [Fact]
    public void Con_rnc_comprador_usa_la_variante_completa_ConsultaTimbre_con_fechas()
    {
        var url = GeneradorUrlConsultaQr.Construir(
            Opciones,
            rncEmisor: "130123456",
            rncComprador: "101234567",
            numeroNcf: "E310001170280",
            fechaEmision: new DateTime(2026, 3, 5),
            montoTotal: 2500m,
            fechaFirma: new DateTime(2026, 3, 5, 14, 30, 45),
            codigoSeguridad: "XYZ999");

        Assert.StartsWith(Opciones.UrlConsultaTimbre, url);
        Assert.Contains("RNCEmisor=130123456", url);
        Assert.Contains("RncComprador=101234567", url);
        Assert.Contains("ENCF=E310001170280", url);
        Assert.Contains("FechaEmision=05-03-2026", url);
        Assert.Contains("MontoTotal=2500.00", url);
        Assert.Contains("FechaFirma=05-03-2026%2014%3A30%3A45", url);
        Assert.Contains("CodigoSeguridad=XYZ999", url);
    }

    [Fact]
    public void Rnc_comprador_vacio_o_solo_espacios_se_trata_igual_que_nulo()
    {
        var url = GeneradorUrlConsultaQr.Construir(
            Opciones, "130123456", "   ", "E320001170280", DateTime.Today, 100m, DateTime.Now, "ABC123");

        Assert.StartsWith(Opciones.UrlConsultaTimbreFc, url);
    }

    [Fact]
    public void Escapa_caracteres_especiales_en_los_parametros()
    {
        var url = GeneradorUrlConsultaQr.Construir(
            Opciones, "130123456", null, "E32 0001&170280", DateTime.Today, 100m, DateTime.Now, "ABC123");

        Assert.DoesNotContain("E32 0001&170280", url); // sin escapar no debería aparecer tal cual
        Assert.Contains(Uri.EscapeDataString("E32 0001&170280"), url);
    }
}
