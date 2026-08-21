using System.Security.Cryptography;
using System.Text;
using FacturacionElectronicaDGII.Modelos;

namespace FacturacionElectronicaDGII;

// ADVERTENCIA: el XSD "RFCE 32 v.1.0.xsd" define el campo CodigoSeguridadeCF como
// un string de exactamente 6 caracteres ("Hash generado en factura de consumo
// original"), pero NO especifica el algoritmo exacto (qué campos se concatenan,
// qué función hash, cómo se codifica el resultado a 6 caracteres). Esa parte vive
// en la documentación técnica de servicios web de DGII, no en los XSD disponibles
// en este proyecto. La implementación de abajo es un PLACEHOLDER funcional (hash
// SHA-256 de los campos clave del comprobante, truncado a 6 caracteres alfanuméricos
// en mayúscula) — NO está verificado contra la especificación oficial de DGII y
// DEBE confirmarse/ajustarse antes de usarse contra el ambiente de certificación real.
public static class GeneradorCodigoSeguridad
{
    public static string Generar(ComprobanteDto comprobante)
    {
        var insumo = string.Join('|',
            comprobante.Emisor.Rnc,
            comprobante.NumeroNcf,
            comprobante.FechaEmision.ToString("yyyyMMdd"),
            comprobante.Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(insumo));
        var alfanumerico = Convert.ToHexString(hash); // solo [0-9A-F], cumple con .{6}

        return alfanumerico[..6];
    }
}
