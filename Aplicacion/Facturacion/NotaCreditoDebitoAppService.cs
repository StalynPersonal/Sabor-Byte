using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Facturacion.Dtos;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Facturacion;

namespace SaborByte.Aplicacion.Facturacion;

public class NotaCreditoDebitoAppService(IAppDbContext db, IAuditoriaService auditoria)
{
    public async Task<NotaCreditoDebitoDto> CrearAsync(
        Guid sucursalId, Guid usuarioId, CrearNotaRequestDto request, CancellationToken ct = default)
    {
        var facturaOriginal = await db.Facturas.FirstOrDefaultAsync(
            f => f.Id == request.FacturaOriginalId && f.SucursalId == sucursalId, ct)
            ?? throw new InvalidOperationException("La factura original no existe.");

        if (request.Monto <= 0)
            throw new InvalidOperationException("El monto de la nota debe ser mayor a cero.");

        if (request.Tipo == TipoNota.Credito && request.Monto > facturaOriginal.Total)
            throw new InvalidOperationException("El monto de la nota de crédito no puede superar el total de la factura original.");

        var nota = new NotaCreditoDebito
        {
            SucursalId = sucursalId,
            FacturaOriginalId = facturaOriginal.Id,
            Tipo = request.Tipo,
            Motivo = request.Motivo,
            Monto = request.Monto,
            CreadoPorUsuarioId = usuarioId
        };

        await AsignarNcfSiAplicaAsync(sucursalId, nota, ct);

        db.NotasCreditoDebito.Add(nota);
        await db.SaveChangesAsync(ct);

        await auditoria.RegistrarAsync(sucursalId, usuarioId, $"Emitir{request.Tipo}", "NotaCreditoDebito", nota.Id,
            $"Factura original: {facturaOriginal.Id}; Motivo: {request.Motivo}; Monto: {request.Monto:0.00}", ct);

        return new NotaCreditoDebitoDto
        {
            Id = nota.Id,
            FacturaOriginalId = nota.FacturaOriginalId,
            Tipo = nota.Tipo,
            NumeroNcf = nota.NumeroNcf,
            Monto = nota.Monto,
            FechaEmision = nota.FechaEmision
        };
    }

    public async Task<List<NotaCreditoDebitoDto>> ListarPorFacturaAsync(Guid facturaId, CancellationToken ct = default) =>
        await db.NotasCreditoDebito
            .Where(n => n.FacturaOriginalId == facturaId)
            .OrderByDescending(n => n.FechaEmision)
            .Select(n => new NotaCreditoDebitoDto
            {
                Id = n.Id,
                FacturaOriginalId = n.FacturaOriginalId,
                Tipo = n.Tipo,
                NumeroNcf = n.NumeroNcf,
                Monto = n.Monto,
                FechaEmision = n.FechaEmision
            })
            .ToListAsync(ct);

    // Igual que la factura original: si hay secuencia NCF tradicional activa para el
    // tipo 33/34, se asigna; si no, la nota queda "sin NCF" sin bloquear la operación.
    // e-CF real (Fase 4) sigue pendiente por las mismas razones que en VentaAppService.
    private async Task AsignarNcfSiAplicaAsync(Guid sucursalId, NotaCreditoDebito nota, CancellationToken ct)
    {
        var tipoComprobante = nota.Tipo == TipoNota.Credito ? "34" : "33";

        var secuencia = await db.SecuenciasNcf.FirstOrDefaultAsync(s =>
            s.SucursalId == sucursalId &&
            s.TipoComprobante == tipoComprobante &&
            s.Activa &&
            s.FechaVencimiento > DateTime.UtcNow &&
            s.SecuenciaProxima <= s.SecuenciaFinal, ct);

        if (secuencia is null)
        {
            nota.EstadoDgii = EstadoDgii.NoAplica;
            return;
        }

        nota.NumeroNcf = secuencia.FormatearNumero(secuencia.SecuenciaProxima);
        nota.TipoComprobante = secuencia.TipoComprobante;
        nota.EstadoDgii = EstadoDgii.NoAplica;
        secuencia.SecuenciaProxima++;
    }
}
