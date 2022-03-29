using Microsoft.Extensions.Configuration;
using Natom.Extensions.Configuration.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Configuration.Services
{
    public class ConfigurationService
    {
        private ConfigRepository _repository;
        private Dictionary<string, string> _config;
        private bool _refreshing = false;

        public ConfigurationService(IConfiguration configuration)
        {
            _repository = new ConfigRepository(configuration["ConnectionStrings:DbConfig"]);
            _config = new Dictionary<string, string>();
        }

        public async Task RefreshAsync()
        {
            _refreshing = true;
            var configData = await _repository.GetConfigAsync();
            var newConfig = new Dictionary<string, string>();
            foreach (var config in configData)
                newConfig.Add(config.Clave.ToLower(), config.Valor);
            _config = newConfig;
            _refreshing = false;
        }

        public async Task<string> GetValueAsync(string key)
        {
            while (_config == null || _config.Count == 0)
            {
                if (!_refreshing) await RefreshAsync();
                await Task.Delay(50);
            }

            return _config[key.ToLower()];
        }
    }
}
