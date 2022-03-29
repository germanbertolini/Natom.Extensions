using Natom.Extensions.MQ.Entities;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Services
{
    public class MQService
    {
        private readonly MQProducerService _mqService;
        private readonly MQContingencyService _mqContingencyService;

        private readonly string _contingencyPath;

        public MQService(IServiceProvider serviceProvider)
        {
            _mqContingencyService = (MQContingencyService)serviceProvider.GetService(typeof(MQContingencyService));
            _mqService = (MQProducerService)serviceProvider.GetService(typeof(MQProducerService));

            _contingencyPath = $"{Directory.GetCurrentDirectory()}\\Contingency\\MessagesMQ\\";
        }

        public async Task PublishAsync(MessageMQ message, QueueMQ queue)
        {
            try
            {
                var retryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2));

                await retryPolicy.ExecuteAsync(async () =>
                {
                    await _mqService.PublishMsgAsync(message, queue);
                });

                if (Directory.Exists(_contingencyPath) && Directory.GetFiles(_contingencyPath).Length > 0 && !_mqContingencyService.EstaPublicando())
                    _ = _mqContingencyService.StartAsync();
            }
            catch (Exception ex)
            {
                await ContingencyStorageAsync(message, queue);
            }
        }

        private async Task ContingencyStorageAsync(MessageMQ message, QueueMQ queue)
        {
            if (!Directory.Exists(_contingencyPath))
                Directory.CreateDirectory(_contingencyPath);

            var content = JsonConvert.SerializeObject(new ContingencyMQ
            {
                QueueMQ = queue,
                MessageMQ = message
            });
            await File.WriteAllTextAsync($"{_contingencyPath}{Guid.NewGuid():N}", content);
        }
    }
}
