using Microsoft.Extensions.DependencyInjection;
using Natom.Extensions.Configuration.PackageConfig;
using Natom.Extensions.Configuration.HostedServices;
using Natom.Extensions.Configuration.Services;

namespace Natom.Extensions
{
    public static class StartupExtensions
    {
        private static bool _hostedServiceAdded = false;

        public static IServiceCollection AddConfigurationService(this IServiceCollection service, int refreshTimeMS = 30000)
        {
            service.AddSingleton<ConfigurationService>();
            service.AddSingleton(new ConfigurationServiceConfig
            {
                RefreshTimeMS = refreshTimeMS
            });

            if (!_hostedServiceAdded)
            {
                service.AddHostedService<ConfigTimedHostedService>();
                _hostedServiceAdded = true;
            }
            return service;
        }
    }
}
