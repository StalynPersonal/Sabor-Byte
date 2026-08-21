using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FacturacionElectronicaDGII.Modelos;
using FacturacionElectronicaDGII.Modelos.Ecf32;

namespace FacturacionElectronicaDGII;

// Genera el XML del e-CF tipo 32 (Comprobante de Consumo Electrónico) según el XSD
// oficial de DGII. Cubre los campos relevantes para un POS de restaurante; el XSD
// completo tiene secciones opcionales adicionales (Transporte, Minería, Paginación,
// Subtotales, OtraMoneda...) no usadas aquí porque no aplican a este tipo de negocio.
public static class GeneradorXmlEcf32
{
    public static string Generar(ComprobanteDto dto)
    {
        var ecf = new Ecf32
        {
            Encabezado = new Encabezado
            {
                IdDoc = new IdDoc
                {
                    ENCF = dto.NumeroNcf,
                    TipoIngresos = "01", // 01 = Ingresos por operaciones (catálogo DGII) — ajustar si aplica otro código
                    TipoPago = 1,
                    TablaFormasPago =
                    [
                        new FormaDePago { FormaPago = 1, MontoPago = dto.Total }
                    ]
                },
                Emisor = new Emisor
                {
                    RNCEmisor = dto.Emisor.Rnc,
                    RazonSocialEmisor = dto.Emisor.RazonSocial,
                    DireccionEmisor = "N/D", // completar con la dirección real de la sucursal emisora
                    FechaEmision = dto.FechaEmision.ToString("dd-MM-yyyy")
                },
                Comprador = dto.Comprador is null ? null : new Comprador
                {
                    RNCComprador = dto.Comprador.RncOCedula,
                    RazonSocialComprador = dto.Comprador.NombreORazonSocial
                },
                Totales = new Totales
                {
                    MontoGravadoTotal = dto.Subtotal,
                    TotalITBIS = dto.MontoImpuestos,
                    MontoTotal = dto.Total
                }
            },
            DetallesItems = dto.Detalle.Select((linea, indice) => new Item
            {
                NumeroLinea = indice + 1,
                IndicadorFacturacion = DeterminarIndicadorFacturacion(linea.TasaItbis),
                NombreItem = linea.Descripcion,
                IndicadorBienoServicio = 2,
                CantidadItem = linea.Cantidad,
                PrecioUnitarioItem = linea.PrecioUnitario,
                MontoItem = linea.Total
            }).ToList(),
            FechaHoraFirma = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")
        };

        var serializador = new XmlSerializer(typeof(Ecf32));
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty); // sin namespace, según el XSD

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = false,
            OmitXmlDeclaration = false
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializador.Serialize(writer, ecf, namespaces);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Catálogo DGII: 1 ITBIS1 18%, 2 ITBIS2 16%, 3 ITBIS3 0%, 4 Exento.
    // TasaItbis null = el producto no aplica ITBIS (Exento); un valor (incluyendo 0)
    // significa que sí aplica, a esa tasa.
    private static int DeterminarIndicadorFacturacion(decimal? tasaItbis) => tasaItbis switch
    {
        null => 4,
        0.18m => 1,
        0.16m => 2,
        0m => 3,
        _ => throw new NotSupportedException(
            $"Tasa de ITBIS no soportada por el catálogo DGII (IndicadorFacturacion): {tasaItbis:P0}. Use 18%, 16% o 0%.")
    };
}
