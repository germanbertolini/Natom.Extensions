using Natom.Extensions.Configuration.Services;
using Natom.Extensions.Logger.Entities.Discord.DTO;
using Natom.Extensions.Logger.Helpers;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Natom.Extensions.Logger.Invokers
{
    public class DiscordWebhookInvoker
    {
        protected readonly IHttpClientFactory _clientFactory;
        protected readonly ConfigurationService _configuration;

        public DiscordWebhookInvoker(IServiceProvider serviceProvider)
        {
            _configuration = (ConfigurationService)serviceProvider.GetService(typeof(ConfigurationService));
            _clientFactory = (IHttpClientFactory)serviceProvider.GetService(typeof(IHttpClientFactory));

            if (_configuration == null)
                throw new Exception("Falta inyectar el servicio ConfigurationService.");
        }

        private async Task<int> GetCancellationTokenDurationMSAsync()
        {
            int toReturn;
            string ms = await _configuration.GetValueAsync("Logging.Discord.WebhookInvoker.CancellationTokenDurationMS");
            if (string.IsNullOrEmpty(ms))
                throw new Exception("Falta configurar 'Logging.Discord.WebhookInvoker.CancellationTokenDurationMS' en la configuración.");
            if (!int.TryParse(ms, out toReturn))
                throw new Exception("Configuración: Entrada 'Logging.Discord.WebhookInvoker.CancellationTokenDurationMS' con valor inválido. Debe ser de tipo INTEGER.");
            return toReturn;
        }

        protected AsyncRetryPolicy<TResult> GetRetryPolicy<TResult>(CancellationTokenSource tokenSource)
                                                            => Policy<TResult>
                                                                .Handle<Exception>()
                                                                .WaitAndRetryAsync(
                                                                    retryCount: 3,
                                                                    delay => TimeSpan.FromMilliseconds(500),
                                                                    onRetry: (result, delay, count, context) =>
                                                                    {
                                                                        var ex = result.Exception;
                                                                        if (ex.InnerException?.HResult == -2147467259)
                                                                        {
                                                                            tokenSource.Cancel();
                                                                            throw ex;
                                                                        }
                                                                    }
                                                                );

        private async Task<TResult> DoHttpRequestAsync<TResult>(HttpRequestMessage request)
        {
            HttpClient client = _clientFactory == null ? new HttpClient() : _clientFactory.CreateClient();

            var tokenSource = new CancellationTokenSource(millisecondsDelay: await GetCancellationTokenDurationMSAsync());
            var policy = GetRetryPolicy<TResult>(tokenSource);

            return await policy.ExecuteAsync(async (cancellationToken) =>
            {
                var _request = await request.CloneAsync();
                var _response = await client.SendAsync(_request, cancellationToken);
                return JsonConvert.DeserializeObject<TResult>(await _response.Content.ReadAsStringAsync());
            }, tokenSource.Token);
        }

        protected async Task<string> TrySendMessagePostAsync(string webhookUrl, reqSendMessage message)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(webhookUrl),
                Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
            };

            return await DoHttpRequestAsync<string>(request);
        }
    }
}
