using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Natom.Extensions.Configuration.Services;
using Natom.Extensions.MQ.Services;
using System;
using System.Collections.Generic;

namespace Natom.Extensions
{
    public static class MQExtensions
    {
        public static IServiceCollection AddMQProducerService(this IServiceCollection services)
        {            
            services.AddSingleton(serviceProvider => {

                var configurationService = (ConfigurationService)serviceProvider.GetService(typeof(ConfigurationService));
                if (configurationService == null)
                    throw new Exception("Es necesario inyectar el servicio de ConfigurationService.");

                var hostsRabbit = new List<HostConfiguration>();
                hostsRabbit.Add(
                    new HostConfiguration
                    {
                        Host = configurationService.GetValueAsync("RabbitMQ.Host").GetAwaiter().GetResult(),
                        Port = Convert.ToUInt16(configurationService.GetValueAsync("RabbitMQ.Port").GetAwaiter().GetResult()),
                        Ssl = { Enabled = Convert.ToBoolean(configurationService.GetValueAsync("RabbitMQ.EnabbleSSL").GetAwaiter().GetResult()) }
                    });

                var cConfig = new ConnectionConfiguration()
                {
                    Hosts = hostsRabbit,
                    UserName = configurationService.GetValueAsync("RabbitMQ.UserName").GetAwaiter().GetResult(),
                    Password = configurationService.GetValueAsync("RabbitMQ.Password").GetAwaiter().GetResult(),
                    VirtualHost = "/",
                    PublisherConfirms = true
                };
                return RabbitHutch.CreateBus(cConfig, service => service.Register(l => l.CreateScope())).Advanced;
            });

            //VA COMO TRANSIENT YA QUE LO CONSUME MQService (Scoped) Y EL PublicadorDeMensajesService (Singleton) A TRAVES DE UNA NUEVA INSTANCIA MQRabiitService, POR FUERA DEL INYECTOR.
            services.AddTransient(serviceProvider => new MQProducerService(serviceProvider));
            services.AddScoped<MQService>();
            services.AddSingleton<MQContingencyService>();

            return services;
        }

        public static IServiceCollection AddMQConsumerService(this IServiceCollection services)
        {
            services.AddSingleton(serviceProvider => new MQConsumerService(serviceProvider));

            return services;
        }
    }
}
