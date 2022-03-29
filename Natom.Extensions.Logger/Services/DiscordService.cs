using Natom.Extensions.Logger.Entities.Discord.DTO;
using Natom.Extensions.Logger.Helpers;
using Natom.Extensions.Logger.Invokers;
using Natom.Extensions.Logger.PackageConfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Logger.Services
{
    public class DiscordService : DiscordWebhookInvoker
    {
        private Dictionary<string, DateTime> _sentExceptionsHistory;
        private readonly LoggerServiceConfig _config;

        public DiscordService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _config = (LoggerServiceConfig)serviceProvider.GetService(typeof(LoggerServiceConfig));
            _sentExceptionsHistory = new Dictionary<string, DateTime>();
        }

        /// <summary>
        /// Chequea que toda la configuración esté OK para mandar el Log al Webhook
        /// </summary>
        private async Task<bool> IsLoggingEnabledAsync(string key)
        {
            bool generalLoggingEnabled = Convert.ToBoolean(await _configuration.GetValueAsync("Logging.Discord.EnableLog"));
            if (!generalLoggingEnabled)
                return false;

            bool infoLoggingEnabled = Convert.ToBoolean(await _configuration.GetValueAsync($"Logging.Discord.{key}.EnableLog"));
            if (!infoLoggingEnabled)
                return false;

            return true;
        }

        /// <summary>
        /// Chequea que toda la configuración esté OK para mandar el Log al Webhook
        /// </summary>
        private async Task<string> GetWebhookUrlAsync(string key)
        {
            string webhookUrl = null;

            webhookUrl = await _configuration.GetValueAsync($"Logging.Discord.{key}.WebhookUrl");
            if (string.IsNullOrEmpty(webhookUrl))
                throw new Exception($"Falta configurar 'Logging.Discord.{key}.WebhookUrl' en la configuración.");

            return webhookUrl;
        }

        /// <summary>
        /// Log de tipo 'INFO' en Discord.
        /// </summary>
        public async Task LogInfoAsync(string message, string traceTransactionId = null, object traceTransactionData = null, string razon = null)
        {
            string webhookUrl;
            if (!await IsLoggingEnabledAsync(key: "Info"))
                return;

            webhookUrl = await GetWebhookUrlAsync(key: "Info");

            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"*{_config.SystemName}*         **INFO**");
            contentBuilder.AppendLine($"- **Message: {message}**");
            if (!string.IsNullOrEmpty(razon))
                contentBuilder.AppendLine($"- **Reason: {razon}**");
            contentBuilder.AppendLine($"- **Date time:** {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");

            if (!string.IsNullOrEmpty(traceTransactionId))
                contentBuilder.AppendLine($"- **Transaction ID:** {traceTransactionId}");

            if (traceTransactionData != null)
                contentBuilder.AppendLine($"- **Transaction data:** {JsonConvert.SerializeObject(traceTransactionData)}");

            contentBuilder.AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");

            await SendMessage(webhookUrl, contentBuilder.ToString());
        }

        /// <summary>
        /// Log de tipo 'EXCEPTION' en Discord.
        /// </summary>
        public async Task LogExceptionAsync(Exception ex, string traceTransactionId = null, object traceTransactionData = null)
        {
            if (ExceptionWasSentInLast30Minutes(ex))
                return;

            string webhookUrl;
            if (!await IsLoggingEnabledAsync(key: "Exception"))
                return;

            webhookUrl = await GetWebhookUrlAsync(key: "Exception");

            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"*{_config.SystemName}*         **EXCEPTION**");
            contentBuilder.AppendLine($"- **Error: {ex.Message}**");
            contentBuilder.AppendLine($"- **Date time:** {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");

            /**** [INICIO] ACOTAMOS STACKTRACE PARA QUE ENTRE EN EL LENGTH PERMITIDO POR DISCORD ****/
            int currentLength = contentBuilder.ToString().Length;
            string stackTrace = ex.StackTrace ?? "";
            bool disableTransactionData = false;
            if (stackTrace.Length + currentLength + 300 > 1999)
            {
                int stackTraceLength = 1999 - currentLength - 300;
                stackTrace = stackTrace.Substring(0, stackTraceLength - 5) + "(...)";
                disableTransactionData = true;
            }
            contentBuilder.AppendLine($"- **Stack trace:** {stackTrace}");
            /**** [FIN] ACOTAMOS STACKTRACE PARA QUE ENTRE EN EL LENGTH PERMITIDO POR DISCORD ****/

            if (!string.IsNullOrEmpty(traceTransactionId))
                contentBuilder.AppendLine($"- **Transaction ID:** {traceTransactionId}");

            if (!disableTransactionData && traceTransactionData != null)
                contentBuilder.AppendLine($"- **Transaction data:** {JsonConvert.SerializeObject(traceTransactionData)}");

            contentBuilder.AppendLine($"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");

            await SendMessage(webhookUrl, contentBuilder.ToString());
        }

        private bool ExceptionWasSentInLast30Minutes(Exception ex)
        {
            bool sent = false;
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                //LIMPIAMOS EL DICTIONARY
                var keys = _sentExceptionsHistory.Keys.ToList();
                foreach (var key in keys)
                    if (_sentExceptionsHistory[key].AddMinutes(30) < DateTime.Now)
                        _sentExceptionsHistory.Remove(key);

                //AHORA NOS FIJAMOS QUE NO ESTE LA CLAVE. SI ESTÁ, ES PORQUE SE MANDÓ HACE MENOS DE 30 MINUTOS
                string exIdentity = EncryptationHelper.CreateMD5(ex.StackTrace);
                if (!_sentExceptionsHistory.ContainsKey(exIdentity))
                    _sentExceptionsHistory.Add(exIdentity, DateTime.Now);
                else
                    sent = true;
            }
            return sent;
        }

        /// <summary>
        /// Log de tipo 'SERVICIO OFFLINE' en Discord.
        /// </summary>
        public async Task LogOfflineAsync(string serviceUrl)
        {
            string webhookUrl;
            if (!await IsLoggingEnabledAsync(key: "ServiceStatus"))
                return;

            webhookUrl = await GetWebhookUrlAsync(key: "ServiceStatus");

            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"*{_config.SystemName}*         :octagonal_sign: **OFFLINE**");
            contentBuilder.AppendLine($"- **Message: Servicio fuera de linea.**");
            contentBuilder.AppendLine($"- **Service URL: {serviceUrl}**");
            contentBuilder.AppendLine($"- **Date time:** {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            contentBuilder.AppendLine($"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");

            await SendMessage(webhookUrl, contentBuilder.ToString());
        }

        /// <summary>
        /// Log de tipo 'STARTED' en Discord.
        /// </summary>
        public async Task LogStartedAsync()
        {
            string webhookUrl;
            if (!await IsLoggingEnabledAsync(key: "ServiceStatus"))
                return;

            webhookUrl = await GetWebhookUrlAsync(key: "ServiceStatus");

            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"*{_config.SystemName}*         :arrow_forward: **STARTED**");
            contentBuilder.AppendLine($"- **Message: Sistema iniciado.**");
            contentBuilder.AppendLine($"- **Date time:** {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            contentBuilder.AppendLine($"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");

            await SendMessage(webhookUrl, contentBuilder.ToString());
        }

        /// <summary>
        /// Log de tipo 'STOPPED' en Discord.
        /// </summary>
        public async Task LogStoppedAsync()
        {
            string webhookUrl;
            if (!await IsLoggingEnabledAsync(key: "ServiceStatus"))
                return;

            webhookUrl = await GetWebhookUrlAsync(key: "ServiceStatus");

            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"*{_config.SystemName}*         :stop_button: **STOPPED**");
            contentBuilder.AppendLine($"- **Message: Sistema detenido.**");
            contentBuilder.AppendLine($"- **Date time:** {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            contentBuilder.AppendLine($"- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");

            await SendMessage(webhookUrl, contentBuilder.ToString());
        }

        /// <summary>
        /// Aseguramos el length del mensaje y lo disparamos!
        /// </summary>
        private async Task SendMessage(string webhookUrl, string content)
        {
            if (content.Length >= 1999) content = content.Substring(0, 1999 - 5) + "(...)";

            await TrySendMessagePostAsync(webhookUrl, new reqSendMessage
            {
                UserName = _config.SystemName,
                Content = content
            });
        }
    }
}
