using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SaborByte.Dominio.Facturacion;
using SaborByte.Infraestructura.Persistencia;

namespace SaborByte.Api.Salud;

// Health check propio (ver sección 9 del plan): permite ver desde /health si hay
// comprobantes e-CF atascados en Contingencia (pendientes de envío a DGII).
public class FacturacionPendienteHealthCheck(SaborByteDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var pendientes = await db.Facturas.CountAsync(f => f.EstadoDgii == EstadoDgii.Contingencia, cancellationToken);

        if (pendientes == 0)
            return HealthCheckResult.Healthy("Sin comprobantes e-CF pendientes de envío.");

        var datos = new Dictionary<string, object> { ["comprobantesPendientes"] = pendientes };

        return pendientes > 20
            ? HealthCheckResult.Unhealthy($"{pendientes} comprobantes e-CF pendientes de envío a DGII.", data: datos)
            : HealthCheckResult.Degraded($"{pendientes} comprobantes e-CF pendientes de envío a DGII.", data: datos);
    }
}
