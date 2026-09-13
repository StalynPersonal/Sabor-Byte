using System.Globalization;

namespace FacturacionElectronicaDGII;

// Arma la URL que se codifica en el QR impreso en el comprobante — es la misma URL con la
// que la DGII o el propio cliente pueden verificar el e-CF desde su portal. Formato
// confirmado contra un integrador de referencia (carpeta /xsd/ValidadorAndromeda), no
// inventado: cuando se conoce el RNC del comprador se usa "ConsultaTimbre" (con
// FechaEmision/FechaFirma); cuando no (cliente de consumo sin RNC) se usa la variante
// simplificada "ConsultaTimbreFC", que no exige esos campos.
public static class GeneradorUrlConsultaQr
{
    public static string Construir(
        FacturacionElectronicaOpciones opciones,
        string rncEmisor,
        string? rncComprador,
        string numeroNcf,
        DateTime fechaEmision,
        decimal montoTotal,
        DateTime fechaFirma,
        string codigoSeguridad)
    {
        var monto = montoTotal.ToString("F2", CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(rncComprador))
        {
            return $"{opciones.UrlConsultaTimbreFc}" +
                   $"?RNCEmisor={Uri.EscapeDataString(rncEmisor)}" +
                   $"&ENCF={Uri.EscapeDataString(numeroNcf)}" +
                   $"&MontoTotal={monto}" +
                   $"&CodigoSeguridad={Uri.EscapeDataString(codigoSeguridad)}";
        }

        return $"{opciones.UrlConsultaTimbre}" +
               $"?RNCEmisor={Uri.EscapeDataString(rncEmisor)}" +
               $"&RncComprador={Uri.EscapeDataString(rncComprador)}" +
               $"&ENCF={Uri.EscapeDataString(numeroNcf)}" +
               $"&FechaEmision={fechaEmision:dd-MM-yyyy}" +
               $"&MontoTotal={monto}" +
               $"&FechaFirma={Uri.EscapeDataString(fechaFirma.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture))}" +
               $"&CodigoSeguridad={Uri.EscapeDataString(codigoSeguridad)}";
    }
}
