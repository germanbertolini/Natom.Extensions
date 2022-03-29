using Microsoft.Extensions.Hosting;
using Natom.Extensions.Configuration.PackageConfig;
using Natom.Extensions.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Natom.Extensions.Configuration.HostedServices
{
    public class ConfigTimedHostedService : IHostedService, IDisposable
    {
        private readonly ConfigurationService _configService;
        private readonly ConfigurationServiceConfig _config;

        private int executionCount = 0;
        private Timer _timer;

        public ConfigTimedHostedService(IServiceProvider serviceProvider)
        {
            _configService = (ConfigurationService)serviceProvider.GetService(typeof(ConfigurationService));
            _config = (ConfigurationServiceConfig)serviceProvider.GetService(typeof(ConfigurationServiceConfig));
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, 0, _config.RefreshTimeMS);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            try
            {
                if (_configService != null)
                    _configService.RefreshAsync().Wait();
            }
            catch (Exception ex)
            {

            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
