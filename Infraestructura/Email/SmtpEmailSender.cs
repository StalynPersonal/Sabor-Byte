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

        // La dirección real del From siempre tiene que ser el buzón autenticado
        // (SmtpUsuario), o MailAddress lanza FormatException si se le pasa algo sin forma
        // de email. El nombre para mostrar es el de la Empresa (sistema multisucursal, una
        // sola Empresa — ver Empresa.cs), no un texto libre por sucursal.
        var direccionRemitente = !string.IsNullOrWhiteSpace(sucursal.SmtpUsuario)
            ? sucursal.SmtpUsuario
            : "no-reply@saborbyte.local";
        var nombreEmpresa = await db.Empresas.Select(e => e.Nombre).FirstOrDefaultAsync(ct);

        using var mensaje = new MailMessage
        {
            From = new MailAddress(direccionRemitente, nombreEmpresa),
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
