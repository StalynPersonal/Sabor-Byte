using System.Globalization;

namespace SaborByte.Web.Api;

// Fija el separador decimal en punto (15.00) sin importar el idioma configurado en el
// navegador del usuario — antes .NET tomaba la cultura del navegador y algunos (es-ES,
// es genérico) usan coma como separador decimal (15,00), inconsistente entre equipos.
public static class CulturaApp
{
    public static void Aplicar()
    {
        var cultura = (CultureInfo)CultureInfo.GetCultureInfo("es-DO").Clone();
        cultura.NumberFormat.NumberDecimalSeparator = ".";
        cultura.NumberFormat.NumberGroupSeparator = ",";
        cultura.NumberFormat.CurrencyDecimalSeparator = ".";
        cultura.NumberFormat.CurrencyGroupSeparator = ",";

        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;
    }
}
