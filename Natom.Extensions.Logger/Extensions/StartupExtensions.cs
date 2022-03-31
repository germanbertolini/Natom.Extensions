using Microsoft.Extensions.DependencyInjection;
using Natom.Extensions.Logger.PackageConfig;
using Natom.Extensions.Logger.HostedServices;
using Natom.Extensions.Logger.Services;
using Natom.Extensions.Logger.Entities;

namespace Natom.Extensions
{
    public static class StartupExtensions
    {
        private static bool _hostedServiceAdded = false;

        public static IServiceCollection AddLoggerService(this IServiceCollection service, string systemName, int insertEachMS = 30000, int bulkInsertSize = 10000)
        {
            service.AddSingleton<DiscordService>();
            service.AddSingleton<LoggerService>();
            service.AddSingleton(new LoggerServiceConfig
            {
                SystemName = systemName,
                InsertEachMS = insertEachMS,
                BulkInsertSize = bulkInsertSize
            });

            service.AddScoped<Transaction>();

            if (!_hostedServiceAdded)
            {
                service.AddHostedService<LoggerTimedHostedService>();
                _hostedServiceAdded = true;
            }
            return service;
        }
    }
}
