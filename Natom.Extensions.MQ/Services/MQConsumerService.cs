using Natom.Extensions.Configuration.Services;
using Natom.Extensions.Logger.Services;
using Natom.Extensions.MQ.Entities;
using Natom.Extensions.MQ.Exceptions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Services
{
    public class MQConsumerService
    {
        private readonly LoggerService _loggerService;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private readonly string _hostName;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;

        public MQConsumerService(IServiceProvider serviceProvider)
        {
            var configurationService = (ConfigurationService)serviceProvider.GetService(typeof(ConfigurationService));
            if (configurationService == null)
                throw new Exception("Es necesario inyectar el servicio de ConfigurationService.");

            _loggerService = (LoggerService)serviceProvider.GetService(typeof(LoggerService));
            if (_loggerService == null)
                throw new Exception("Es necesario inyectar el servicio de LoggerService.");

            _hostName = configurationService.GetValueAsync("RabbitMQ.Host").GetAwaiter().GetResult();
            _port = Convert.ToUInt16(configurationService.GetValueAsync("RabbitMQ.Port").GetAwaiter().GetResult());
            _userName = configurationService.GetValueAsync("RabbitMQ.UserName").GetAwaiter().GetResult();
            _password = configurationService.GetValueAsync("RabbitMQ.Password").GetAwaiter().GetResult();

            var factory = new ConnectionFactory()
            {
                DispatchConsumersAsync = true, //HABILITA CONSUMER ASINCRÓNICO
                Uri = new Uri($"amqp://{_userName}:{_password}@{_hostName}:{_port}")
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        /// <typeparam name="TMessage">El tipo del mensaje a leer</typeparam>
        /// <param name="queue">Información de la cola a consumir</param>
        /// <param name="qtyToRead">Cantidad de mensajes a consumir</param>
        /// <param name="onReadAsync">El delegado a ejecutar. Debe ser asincrónico.</param>
        /// <param name="cancellationTokens">Todos los CancellationToken para abortar la operación.</param>
        /// <returns></returns>
        public async Task ConsumeMsgAsync<TMessage>(QueueMQ queue, int qtyToRead, int threadNumber,
                                            Func<ReadMessageMQ<TMessage>, CancellationToken, DateTimeOffset, Task> onReadAsync,
                                            Func<ErrorMessageMQ, Task<bool>> onExceptionSendToErrorsQueueHandlerAsync,
                                            DateTimeOffset executionTime,
                                            params CancellationToken[] cancellationTokens)
        {
            using (var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens))
            {
                for (int loop = 0; loop < qtyToRead; loop++)
                {
                    if (linkedCancellationToken.IsCancellationRequested)
                        break;

                    bool noAck = false;
                    BasicGetResult result = _channel.BasicGet(queue.QueueName, noAck);
                    if (result == null) //SI NO HAY MAS MENSAJES
                        break;
                    else
                    {
                        var props = result.BasicProperties;
                        var messageBody = result.Body;
                        var messageBodyStr = Encoding.UTF8.GetString(messageBody.ToArray());
                        long retryCounter = 0;

                    retry:
                        try
                        {
                            var messageBodyObj = JsonConvert.DeserializeObject<TMessage>(messageBodyStr);

                            var message = new ReadMessageMQ<TMessage>
                            {
                                ThreadNumber = threadNumber,
                                CycleNumber = loop + 1,
                                Content = messageBodyObj
                            };

                            if (linkedCancellationToken.IsCancellationRequested)
                                break;


                            await onReadAsync(message, linkedCancellationToken.Token, executionTime);

                            if (retryCounter > 0)
                                _loggerService.LogInfo(transactionId: "", $"Reintento {retryCounter} procesado correctamente.", new { queue, qtyToRead, threadNumber });

                            if (message.MustBeRemoved())
                                _channel.BasicAck(result.DeliveryTag, false);
                        }
                        catch (RetryableException ex)
                        {
                            var ms = ex.GetDelayMiliseconds();
                            _loggerService.LogException(transactionId: "", new Exception($"Se produjo un Exception ({retryCounter + 1} veces). Se realiza un delay de {ms} milisegundos para reintentar procesar."), new { queue, qtyToRead, threadNumber, innerException = ex.GetException().ToString() });
                            await Task.Delay(ms);
                            retryCounter++;
                            goto retry;
                        }
                        catch (Exception ex)
                        {
                            var originalException = ex;

                            if (onExceptionSendToErrorsQueueHandlerAsync != null)
                            {
                                _loggerService.LogException(transactionId: "", new Exception($"Se produjo un Exception. Se procede a encolar el mensaje en la cola de errores"), new { queue, qtyToRead, threadNumber, exception = ex.ToString() });

                                var queueConfirmed = await onExceptionSendToErrorsQueueHandlerAsync(new ErrorMessageMQ
                                {
                                    Message = messageBodyStr,
                                    Error = ex.ToString()
                                });

                                //POR ULTIMO BORRA EL MENSAJE DE LA COLA ORIGINAL
                                if (queueConfirmed)
                                    _channel.BasicAck(result.DeliveryTag, false);
                                else
                                    _loggerService.LogError(transactionId: "", $"No se pudo encolar en cola de errores. Se mantiene en cola original.", new { queue, qtyToRead, threadNumber });
                            }
                            else
                                throw originalException;
                        }
                    }
                }
            }
        }
    }
}
