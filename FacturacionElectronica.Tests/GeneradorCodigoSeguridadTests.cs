using FacturacionElectronicaDGII.Modelos;
using Xunit;

namespace FacturacionElectronicaDGII.Tests;

public class GeneradorCodigoSeguridadTests
{
    [Fact]
    public void Generar_DevuelveSeisCaracteres()
    {
        var comprobante = new ComprobanteDto
        {
            TipoNcf = "32",
            NumeroNcf = "E320000000123",
            FechaEmision = DateTime.UtcNow,
            Emisor = new EmisorDto { Rnc = "130123456", RazonSocial = "Sabor Byte SRL" },
            Total = 590m
        };

        var codigo = GeneradorCodigoSeguridad.Generar(comprobante);

        Assert.Equal(6, codigo.Length);
    }

    [Fact]
    public void Generar_EsDeterministaParaElMismoComprobante()
    {
        var comprobante = new ComprobanteDto
        {
            TipoNcf = "32",
            NumeroNcf = "E320000000123",
            FechaEmision = new DateTime(2026, 8, 21),
            Emisor = new EmisorDto { Rnc = "130123456", RazonSocial = "Sabor Byte SRL" },
            Total = 590m
        };

        var codigo1 = GeneradorCodigoSeguridad.Generar(comprobante);
        var codigo2 = GeneradorCodigoSeguridad.Generar(comprobante);

        Assert.Equal(codigo1, codigo2);
    }
}
