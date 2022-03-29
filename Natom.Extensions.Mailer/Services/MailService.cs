using Natom.Extensions.Auth.Entities.Models;
using Natom.Extensions.Configuration.Services;
using Natom.Extensions.Logger.Entities;
using Natom.Extensions.Logger.PackageConfig;
using Natom.Extensions.Logger.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Mailer.Services
{
    public class MailService
    {
        private readonly LoggerService _logger;
        private readonly ConfigurationService _configuration;

        public MailService(IServiceProvider serviceProvider)
        {
            _logger = (LoggerService)serviceProvider.GetService(typeof(LoggerService));
            _configuration = (ConfigurationService)serviceProvider.GetService(typeof(ConfigurationService));
        }

        public async Task EnviarEmailParaConfirmarRegistroAsync(Transaction transaction, string scope, Usuario usuario)
        {
            string subject = "Confirmar registración";
            string appAddress = await _configuration.GetValueAsync($"{scope}.URL");
            string productName = await _configuration.GetValueAsync("General.ProductName");
            var dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { s = usuario.SecretConfirmacion, e = usuario.Email }));
            var data = Uri.EscapeDataString(Convert.ToBase64String(dataBytes));
            string link = new Uri($"{appAddress}/users/confirm/{data}").AbsoluteUri;
            string body = ResourceTemplates.Default;
            body = body.Replace("$body$", String.Format("<h2>¡Bienvenido a " + productName + "!</h2><br/><p>Por favor, para <b>generar la clave de acceso al sistema</b> haga clic en el siguiente link: <a href='{0}'>{0}</a></p>", link));
            await EnviarMailAsync(transaction, subject, body, usuario.Email, usuario.Nombre);
        }

        public async Task EnviarEmailParaRecuperarClaveAsync(Transaction transaction, string scope, Usuario usuario)
        {
            string subject = "Recuperar clave";
            string appAddress = await _configuration.GetValueAsync($"{scope}.URL");
            string productName = await _configuration.GetValueAsync("General.ProductName");
            var dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { s = usuario.SecretConfirmacion, e = usuario.Email }));
            var data = Uri.EscapeDataString(Convert.ToBase64String(dataBytes));
            string link = new Uri($"{appAddress}/users/confirm/{data}").AbsoluteUri;
            string body = ResourceTemplates.Default;
            body = body.Replace("$body$", String.Format("<h2>Recupero de clave " + productName + "</h2><br/><p>Por favor, para <b>recuperar la clave de acceso al sistema</b> haga clic en el siguiente link: <a href='{0}'>{0}</a></p>", link));
            await EnviarMailAsync(transaction, subject, body, usuario.Email, usuario.Nombre);
        }

        public async Task EnviarMailAsync(Transaction transaction, string asunto, string htmlBody, string emailDestinatario, string nombreDestinatario = null)
        {
            _logger.LogInfo(transaction?.TraceTransactionId, "Comienzo envío email", new { emailDestinatario, nombreDestinatario, asunto, htmlBody });

            try
            {
                var smtpUser = await _configuration.GetValueAsync("Mailing.SMTP.User");
                var smtpPassword = await _configuration.GetValueAsync("Mailing.SMTP.Password");
                var smtpSenderName = await _configuration.GetValueAsync("Mailing.SenderName");

                var fromAddress = new MailAddress(smtpUser, smtpSenderName);
                var toAddress = new MailAddress(emailDestinatario, nombreDestinatario);

                var smtp = new SmtpClient
                {
                    Host = await _configuration.GetValueAsync("Mailing.SMTP.Host"),
                    Port = Convert.ToInt32(await _configuration.GetValueAsync("Mailing.SMTP.Port")),
                    EnableSsl = Convert.ToBoolean(await _configuration.GetValueAsync("Mailing.SMTP.EnableSSL")),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = asunto,
                    Body = htmlBody
                })
                {
                    await smtp.SendMailAsync(message);

                    _logger.LogInfo(transaction?.TraceTransactionId, "Email enviado correctamente", new { emailDestinatario, nombreDestinatario });
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(transaction?.TraceTransactionId, ex, new { emailDestinatario, nombreDestinatario, asunto, htmlBody });
                throw ex;
            }
        }
    }
}
