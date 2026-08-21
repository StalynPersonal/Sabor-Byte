using FacturacionElectronicaDGII;
using SaborByte.Aplicacion.Interfaces;

namespace SaborByte.Infraestructura.Facturacion;

// Adaptador: traduce entre el dominio de Sabor Byte y la librería independiente
// FacturacionElectronicaDGII. Se invoca únicamente cuando Sucursal.EcfActivo = true.
public class FacturacionElectronicaGateway(IServicioFacturacionElectronica servicioEcf) : IFacturacionElectronicaGateway
{
    public async Task<ResultadoEmisionEcf> EmitirAsync(Guid facturaId, CancellationToken ct = default)
    {
        // TODO (Fase 4): cargar la Factura + FacturaDetalle, asignar SecuenciaNcf,
        // mapear a ComprobanteDto, validar, firmar y enviar usando servicioEcf.
        throw new NotImplementedException(
            "La emisión real de e-CF se implementa en la Fase 4, una vez conectado el certificado y el ambiente DGII.");
    }
}
