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

        Dominio.Clientes.Cliente? cliente = factura.ClienteId is null
            ? null
            : await db.Clientes.FirstOrDefaultAsync(c => c.Id == factura.ClienteId, ct);

        var comprobante = new ComprobanteDto
        {
            TipoNcf = factura.TipoComprobante ?? "32",
            NumeroNcf = factura.NumeroNcf ?? string.Empty,
            FechaEmision = factura.FechaEmision,
            Emisor = new EmisorDto
            {
                Rnc = sucursal.Rnc ?? string.Empty,
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

        try
        {
            var resultado = await servicioEcf.EnviarADgiiAsync(xmlFirmado, ct);
            factura.EstadoDgii = resultado.Estado == "Aceptado" ? EstadoDgii.Aceptado : EstadoDgii.Rechazado;
            await db.SaveChangesAsync(ct);

            return new ResultadoEmisionEcf
            {
                Exitoso = true,
                TrackId = resultado.TrackId,
                EstadoDgii = resultado.Estado,
                MensajeDgii = resultado.Mensaje
            };
        }
        catch (NotImplementedException)
        {
            // El envío real a DGII aún no está implementado (ver ServicioFacturacionElectronicaDgii).
            // El comprobante ya quedó validado, generado y firmado — se marca en contingencia
            // para reintentar el envío más adelante, sin bloquear la venta.
            factura.EstadoDgii = EstadoDgii.Contingencia;
            await db.SaveChangesAsync(ct);

            return new ResultadoEmisionEcf
            {
                Exitoso = true,
                EstadoDgii = "Contingencia",
                MensajeDgii = "Comprobante generado y firmado; envío a DGII pendiente de implementar."
            };
        }
    }
}
