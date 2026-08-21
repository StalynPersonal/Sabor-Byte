using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Infraestructura.Persistencia;

namespace SaborByte.Infraestructura.Email;

public class SmtpEmailSender(SaborByteDbContext db) : IEmailSender
{
    public async Task EnviarAsync(Guid sucursalId, string destinatario, string asunto, string cuerpoHtml, CancellationToken ct = default)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == sucursalId, ct);

        // Envío opcional: si la sucursal no activó SMTP o falta configuración, no se envía
        // (y no se lanza excepción) — el resto del flujo de negocio no debe depender de esto.
        if (sucursal is null || !sucursal.SmtpActivo || string.IsNullOrWhiteSpace(sucursal.SmtpHost))
            return;

        using var mensaje = new MailMessage
        {
            From = new MailAddress(sucursal.SmtpRemitente ?? sucursal.SmtpUsuario ?? "no-reply@saborbyte.local"),
            Subject = asunto,
            Body = cuerpoHtml,
            IsBodyHtml = true
        };
        mensaje.To.Add(destinatario);

        using var cliente = new SmtpClient(sucursal.SmtpHost, sucursal.SmtpPuerto ?? 587)
        {
            EnableSsl = sucursal.SmtpUsaSsl,
            Credentials = string.IsNullOrWhiteSpace(sucursal.SmtpUsuario)
                ? null
                : new NetworkCredential(sucursal.SmtpUsuario, sucursal.SmtpPassword)
        };

        await cliente.SendMailAsync(mensaje, ct);
    }
}
