namespace SaborByte.Web.Api;

// Espejo de Dominio.Comun.HorarioRd (backend) — las 4 apps Blazor no referencian
// Dominio/Aplicacion, así que este helper vive acá para las pantallas que arman un rango
// de fechas y lo mandan a la Api. República Dominicana: UTC-4 fijo, sin horario de verano.
public static class HorarioRd
{
    private static readonly TimeSpan Desfase = TimeSpan.FromHours(-4);

    // Instante UTC correspondiente a la medianoche del día calendario de RD indicado
    // (la parte de hora de fechaLocal se ignora, solo importa la fecha).
    public static DateTime ADateTimeUtc(DateTime fechaLocal) => fechaLocal.Date - Desfase;
}
