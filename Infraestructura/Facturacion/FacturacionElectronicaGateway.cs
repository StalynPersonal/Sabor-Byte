using FacturacionElectronicaDGII;
using FacturacionElectronicaDGII.Modelos;
using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Facturacion;
using SaborByte.Infraestructura.Persistencia;

namespace SaborByte.Infraestructura.Facturacion;

// Adaptador: traduce entre el dominio de Sabor Byte (Factura/FacturaDetalle) y la
// librería independiente FacturacionElectronicaDGII. Se invoca únicamente cuando
// Sucursal.EcfActivo = true.
public class FacturacionElectronicaGateway(
    SaborByteDbContext db, IServicioFacturacionElectronica servicioEcf) : IFacturacionElectronicaGateway
{
    public async Task<ResultadoEmisionEcf> EmitirAsync(Guid facturaId, CancellationToken ct = default)
    {
        var factura = await db.Facturas
            .Include(f => f.Detalle)
            .FirstOrDefaultAsync(f => f.Id == facturaId, ct)
            ?? throw new InvalidOperationException("La factura no existe.");

        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == factura.SucursalId, ct)
            ?? throw new InvalidOperationException("La sucursal de la factura no existe.");

        // El RNC del emisor es el de la Empresa (única en el sistema), no de la sucursal.
        var empresa = await db.Empresas.FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No hay una empresa configurada en el sistema.");

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == factura.ClienteId, ct);

        var comprobante = new ComprobanteDto
        {
            TipoNcf = factura.TipoComprobante ?? "32",
            NumeroNcf = factura.NumeroNcf ?? string.Empty,
            FechaEmision = factura.FechaEmision,
            Emisor = new EmisorDto
            {
                Rnc = empresa.Rnc ?? string.Empty,
                RazonSocial = sucursal.Nombre
            },
            Comprador = cliente is null ? null : new CompradorDto
            {
                RncOCedula = cliente.RncOCedula,
                NombreORazonSocial = cliente.NombreORazonSocial
            },
            Detalle = factura.Detalle.Select(d => new LineaComprobanteDto
            {
                Descripcion = d.NombreProducto,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                TasaItbis = d.TasaItbis,
                Impuesto = d.Itbis,
                Total = d.Total
            }).ToList(),
            Subtotal = factura.Subtotal,
            MontoImpuestos = factura.Itbis,
            Total = factura.Total
        };

        var validacion = servicioEcf.ValidarComprobante(comprobante);
        if (!validacion.EsValido)
            return new ResultadoEmisionEcf { Exitoso = false, ErroresValidacion = validacion.Errores };

        string xmlFirmado;
        try
        {
            xmlFirmado = await servicioEcf.FirmarComprobanteAsync(comprobante, ct);
        }
        catch (InvalidOperationException ex)
        {
            // Certificado no configurado (ej. ambiente de desarrollo sin certificado real todavía).
            return new ResultadoEmisionEcf { Exitoso = false, ErroresValidacion = [ex.Message] };
        }

        factura.XmlFirmadoDgii = xmlFirmado;
        factura.CodigoSeguridadDgii = servicioEcf.GenerarCodigoSeguridad(xmlFirmado);

        factura.FechaEnvioDgii = DateTime.UtcNow;

        try
        {
            var resultado = await servicioEcf.EnviarADgiiAsync(xmlFirmado, ct);
            factura.TrackIdDgii = resultado.TrackId;
            factura.MensajeDgii = resultado.Mensaje;
            factura.FechaRespuestaDgii = DateTime.UtcNow;
            // El acuse de recibo (ARECF) solo confirma que DGII recibió el envío — el
            // estado final (Aceptado/Rechazado/Condicional) se obtiene después vía
            // ConsultarEstadoAsync con el TrackId, no en esta misma respuesta.
            factura.EstadoDgii = resultado.Estado switch
            {
                "Aceptado" => EstadoDgii.Aceptado,
                "EnProceso" => EstadoDgii.Pendiente,
                _ => EstadoDgii.Rechazado
            };
            await db.SaveChangesAsync(ct);

            return new ResultadoEmisionEcf
            {
                Exitoso = true,
                TrackId = resultado.TrackId,
                EstadoDgii = resultado.Estado,
                MensajeDgii = resultado.Mensaje,
                CodigoSeguridadDgii = factura.CodigoSeguridadDgii
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Sin conexión a DGII o el servicio no respondió a tiempo. El comprobante ya
            // quedó validado, generado y firmado — se marca en contingencia para
            // reintentar el envío más adelante (ver sección 4 del plan), sin bloquear la venta.
            var mensaje = $"Comprobante generado y firmado; no se pudo contactar a DGII: {ex.Message}";
            factura.EstadoDgii = EstadoDgii.Contingencia;
            factura.MensajeDgii = mensaje;
            await db.SaveChangesAsync(ct);

            return new ResultadoEmisionEcf
            {
                Exitoso = true,
                EstadoDgii = "Contingencia",
                MensajeDgii = mensaje,
                CodigoSeguridadDgii = factura.CodigoSeguridadDgii
            };
        }
    }
}
