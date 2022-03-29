using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Natom.Extensions.Logger.Services;
using Natom.Extensions.MQ.Entities;
using Natom.Extensions.MQ.Services;
using Natom.Extensions.MQ.WorkerUtilities.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.WorkerUtilities
{
    /// <typeparam name="TWorker">Type del Worker</typeparam>
    /// <typeparam name="TMessage">Type del mensaje a consumir</typeparam>
    /// <typeparam name="TWorkerConfig">Type de la clase de configuración del Worker</typeparam>
    public class CycleWorkerMQ<TWorker, TMessage, TWorkerConfig> : BackgroundService where TWorkerConfig : WorkerMQConfig
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly LoggerService _loggerService;
        protected readonly MQConsumerService _mqConsumerService;
        protected readonly MQProducerService _mqProducerService;

        protected TWorkerConfig _mqWorkerConfig { get; private set; }
        protected DateTimeOffset _firedAt { get; private set; }


        public CycleWorkerMQ(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _loggerService = (LoggerService)serviceProvider.GetService(typeof(LoggerService));
            _mqConsumerService = (MQConsumerService)serviceProvider.GetService(typeof(MQConsumerService));
            _mqProducerService = new MQProducerService(_serviceProvider);

            var configuration = (IConfiguration)serviceProvider.GetService(typeof(IConfiguration));

            //WORKER CONFIG
            _mqWorkerConfig = configuration.GetSection("MQWorker").Get<TWorkerConfig>();
        }

        /// <summary>
        /// Ejecución del Worker
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _loggerService.LogInfo(transactionId: "", $"Worker {_mqWorkerConfig.InstanceName} iniciado.", GetTransactionInfoForWorker(), logOnDiscord: true);

            await Task.Delay(1000);

            while (!stoppingToken.IsCancellationRequested)
            {
                _firedAt = DateTimeOffset.Now;

                try
                {
                    Parallel.For(fromInclusive: 0,
                                    toExclusive: _mqWorkerConfig.Process.Threads,
                                    iThread => DoWorkAsync(iThread + 1, stoppingToken, _firedAt).Wait());
                }
                catch (Exception ex)
                {
                    _loggerService.LogException(transactionId: "", ex, GetTransactionInfoForWorker());
                }

                //AGUARDAMOS A LA PRÓXIMA EJECUCIÓN
                int waitForDelay = _firedAt.Millisecond + (_mqWorkerConfig.Process.MinIntervalMS) - DateTimeOffset.Now.Millisecond;
                if (waitForDelay < 0) waitForDelay = 0;
                await Task.Delay(waitForDelay, stoppingToken);
            }

            _loggerService.LogInfo(transactionId: "", $"Worker {_mqWorkerConfig.InstanceName} detenido.", GetTransactionInfoForWorker(), logOnDiscord: true);
        }

        /// <summary>
        /// Genera una conexión al MQ y llama a consumir los mensajes proveyendo un delegado callback
        /// </summary>
        private async Task DoWorkAsync(int threadNumber, CancellationToken stoppingToken, DateTimeOffset executionTime)
        {
            //DEFINIMOS LA COLA
            var queue = new QueueMQ
            {
                QueueName = _mqWorkerConfig.Queue.QueueName,
                Exchange = _mqWorkerConfig.Queue.Exchange,
                RoutingKey = _mqWorkerConfig.Queue.RoutingKey
            };

            //DEFINIMOS EL HANDLER ANTE UN ERROR PARA QUE LO REENCOLE EN LA COLA DE ERRORES
            Func<ErrorMessageMQ, Task<bool>> handlerExceptionAsync = (messageError) =>
                _mqProducerService.PublishMsgAsync(messageError, new QueueMQ
                {
                    QueueName = $"{queue.QueueName}_error",
                    Exchange = queue.Exchange,
                    RoutingKey = $"{queue.RoutingKey}_error"
                });

            //FINALMENTE LLAMAMOS AL CONSUMER
            await _mqConsumerService.ConsumeMsgAsync<TMessage>(
                                                queue,
                                                qtyToRead: _mqWorkerConfig.Process.MsgReadingQuantity,
                                                threadNumber,
                                                onReadAsync: OnReadMessageAsync,
                                                onExceptionSendToErrorsQueueHandlerAsync: handlerExceptionAsync,
                                                executionTime,
                                                stoppingToken);
        }

        /// <summary>
        /// Evento que es llamado por cada mensaje leído.
        /// </summary>
        /// <returns>True: Eliminar el mensaje de la cola | False: Mantener el mensaje en la cola (ATENCIÓN: será leido en la proxima lectura)</returns>
        public virtual async Task OnReadMessageAsync(ReadMessageMQ<TMessage> message, CancellationToken cancellationToken, DateTimeOffset executionTime)
        {
            throw new NotImplementedException("Falta implementar el manejo de mensaje.");
        }

        /// <summary>
        /// Date: 14/06/2021
        /// Retorna un diccionario con datos de la instancia del Worker para persistirlo en el Logger como 'ExtraFields'
        /// </summary>
        protected object GetTransactionInfoForWorker()
        {
            var transactionInfo = new Dictionary<string, object>
            {
                ["WorkerName"] = _mqWorkerConfig.Name,
                ["WorkerInstance"] = _mqWorkerConfig.InstanceName,
                ["WorkerFullName"] = typeof(TWorker).FullName
            };

            return transactionInfo;
        }
    }
}
