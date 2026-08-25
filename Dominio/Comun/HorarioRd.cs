namespace SaborByte.Dominio.Comun;

// Todas las fechas del sistema se guardan en UTC (Factura.FechaEmision, etc.), pero el
// negocio piensa en día calendario de República Dominicana — UTC-4 fijo, sin horario de
// verano. Usar esto en vez de DateTime.Today/DateTime.UtcNow.Date evita que el "día" se
// corra según la zona horaria del servidor (Azure App Service corre en UTC, 4 horas
// adelante de RD, así que "hoy" en el servidor ya es "mañana" para un cliente en RD desde
// media tarde en adelante).
public static class HorarioRd
{
    private static readonly TimeSpan Desfase = TimeSpan.FromHours(-4);

    // Convierte un instante UTC a la hora de pared de RD (mismo instante, otra representación).
    public static DateTime AHoraLocal(DateTime utc) => utc + Desfase;

    // Instante UTC correspondiente a la medianoche de "hoy" en RD.
    public static DateTime HoyUtc() => AHoraLocal(DateTime.UtcNow).Date - Desfase;

    // Instante UTC correspondiente a la medianoche del día calendario de RD indicado
    // (la parte de hora de fechaLocal se ignora, solo importa la fecha).
    public static DateTime ADateTimeUtc(DateTime fechaLocal) => fechaLocal.Date - Desfase;
}
