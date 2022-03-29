using EasyNetQ;
using EasyNetQ.Topology;
using Natom.Extensions.MQ.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Services
{
    public class MQProducerService
    {
        private readonly IAdvancedBus _advancedBusEasyNetQ;


        public MQProducerService(IServiceProvider serviceProvider)
        {
            _advancedBusEasyNetQ = (IAdvancedBus)serviceProvider.GetService(typeof(IAdvancedBus));
        }

        public async Task<bool> PublishMsgAsync<T>(T message, QueueMQ queue)
        {
            bool confirm = false;
            IExchange exchangeProducer = await _advancedBusEasyNetQ.ExchangeDeclareAsync(queue.Exchange, ExchangeType.Direct);

            var messageProperties = new MessageProperties();
            messageProperties.DeliveryMode = 2; //PERSISTENT

            await _advancedBusEasyNetQ.PublishAsync(exchangeProducer, queue.RoutingKey, true, messageProperties,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message))).ContinueWith(t =>
                {
                    if (t.IsCompleted && t.IsCompletedSuccessfully)
                    {
                        confirm = true;
                    }

                    if (t.IsFaulted)
                    {
                        confirm = false;
                    }
                });
            return confirm;
        }
    }
}
