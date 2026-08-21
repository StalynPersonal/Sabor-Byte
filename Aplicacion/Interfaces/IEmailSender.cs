namespace SaborByte.Aplicacion.Interfaces;

public interface IEmailSender
{
    // sucursalId determina qué configuración SMTP usar (ver Sucursal.SmtpActivo);
    // si el envío no está habilitado para esa sucursal, no hace nada (no lanza error).
    Task EnviarAsync(Guid sucursalId, string destinatario, string asunto, string cuerpoHtml, CancellationToken ct = default);
}
