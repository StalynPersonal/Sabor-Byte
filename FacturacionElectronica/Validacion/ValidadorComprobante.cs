using System.Text.RegularExpressions;
using FacturacionElectronicaDGII.Modelos;

namespace FacturacionElectronicaDGII.Validacion;

// Valida que el comprobante cumpla los requisitos de DGII ANTES de firmarse y enviarse.
// Si el resultado no es válido, el comprobante no debe pasar a firma/envío.
public static partial class ValidadorComprobante
{
    private static readonly Regex PatronRncOCedula = RegexRncOCedula();

    // Tipos de NCF electrónico que requieren que el comprador tenga RNC/Cédula (crédito fiscal, gubernamental, etc.)
    private static readonly HashSet<string> TiposQueRequierenComprador = new(StringComparer.OrdinalIgnoreCase)
    {
        "E31", // Crédito Fiscal
        "E41", // Compras
        "E43", // Gastos Menores (referencial)
        "E44", // Regímenes Especiales
        "E45"  // Gubernamental
    };

    public static ResultadoValidacion Validar(ComprobanteDto comprobante)
    {
        var resultado = new ResultadoValidacion();

        ValidarEmisor(comprobante, resultado);
        ValidarSecuencia(comprobante, resultado);
        ValidarComprador(comprobante, resultado);
        ValidarFechas(comprobante, resultado);
        ValidarDetalleYMontos(comprobante, resultado);

        return resultado;
    }

    private static void ValidarEmisor(ComprobanteDto c, ResultadoValidacion r)
    {
        if (string.IsNullOrWhiteSpace(c.Emisor.Rnc) || !PatronRncOCedula.IsMatch(c.Emisor.Rnc))
            r.AgregarError("El RNC del emisor es obligatorio y debe tener un formato válido (9 u 11 dígitos).");

        if (string.IsNullOrWhiteSpace(c.Emisor.RazonSocial))
            r.AgregarError("La razón social del emisor es obligatoria.");
    }

    private static void ValidarSecuencia(ComprobanteDto c, ResultadoValidacion r)
    {
        if (string.IsNullOrWhiteSpace(c.TipoNcf))
            r.AgregarError("El tipo de NCF/e-CF es obligatorio.");

        if (string.IsNullOrWhiteSpace(c.NumeroNcf))
            r.AgregarError("El número de NCF/e-CF es obligatorio.");

        if (c.FechaVencimientoSecuencia is not null && c.FechaVencimientoSecuencia < c.FechaEmision)
            r.AgregarError("La secuencia de NCF/e-CF está vencida para la fecha de emisión del comprobante.");
    }

    private static void ValidarComprador(ComprobanteDto c, ResultadoValidacion r)
    {
        var requiereComprador = !string.IsNullOrWhiteSpace(c.TipoNcf) &&
                                 TiposQueRequierenComprador.Contains(c.TipoNcf);

        if (!requiereComprador)
            return;

        if (c.Comprador is null || string.IsNullOrWhiteSpace(c.Comprador.RncOCedula))
        {
            r.AgregarError($"El tipo de comprobante {c.TipoNcf} exige que el comprador tenga RNC/Cédula registrado.");
            return;
        }

        if (!PatronRncOCedula.IsMatch(c.Comprador.RncOCedula))
            r.AgregarError("El RNC/Cédula del comprador no tiene un formato válido (9 u 11 dígitos).");
    }

    private static void ValidarFechas(ComprobanteDto c, ResultadoValidacion r)
    {
        if (c.FechaEmision == default)
        {
            r.AgregarError("La fecha de emisión es obligatoria.");
            return;
        }

        if (c.FechaEmision > DateTime.UtcNow.AddDays(1))
            r.AgregarError("La fecha de emisión no puede ser futura.");
    }

    private static void ValidarDetalleYMontos(ComprobanteDto c, ResultadoValidacion r)
    {
        if (c.Detalle.Count == 0)
        {
            r.AgregarError("El comprobante debe tener al menos una línea de detalle.");
            return;
        }

        if (c.Subtotal <= 0)
            r.AgregarError("El subtotal no puede ser vacío ni cero.");

        if (c.Total <= 0)
            r.AgregarError("El monto total no puede ser vacío ni cero.");

        if (c.MontoImpuestos < 0)
            r.AgregarError("El monto de impuestos no puede ser negativo.");

        var totalCalculado = c.Subtotal + c.MontoImpuestos;
        if (Math.Abs(totalCalculado - c.Total) > 0.01m)
            r.AgregarError($"El total ({c.Total:0.00}) no coincide con subtotal + impuestos ({totalCalculado:0.00}).");

        foreach (var linea in c.Detalle)
        {
            if (string.IsNullOrWhiteSpace(linea.Descripcion))
                r.AgregarError("Cada línea de detalle debe tener descripción.");
            if (linea.Cantidad <= 0)
                r.AgregarError($"La línea '{linea.Descripcion}' debe tener una cantidad mayor a cero.");
            if (linea.PrecioUnitario < 0)
                r.AgregarError($"La línea '{linea.Descripcion}' no puede tener un precio unitario negativo.");
        }
    }

    [GeneratedRegex(@"^\d{9}$|^\d{11}$")]
    private static partial Regex RegexRncOCedula();
}
