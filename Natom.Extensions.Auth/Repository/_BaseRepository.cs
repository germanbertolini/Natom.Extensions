using Natom.Extensions.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Repository
{
    public class BaseRepository
    {
        protected readonly ConfigurationService _configurationService;
        protected readonly string _connectionString;

        public BaseRepository(IServiceProvider serviceProvider)
        {
            _configurationService = (ConfigurationService)serviceProvider.GetService(typeof(ConfigurationService));
            if (_configurationService == null)
                throw new Exception("Es necesario inyectar el servicio de ConfigurationService.");

            _connectionString = _configurationService.GetValueAsync("ConnectionStrings.DbSecurity").GetAwaiter().GetResult();
        }
    }
}
