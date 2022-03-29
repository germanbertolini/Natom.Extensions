using Natom.Extensions.MQ.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Services
{
    public class MQContingencyService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _contingencyPath;
        private object _lockStartAsync = new object();

        private bool _running;
        public bool EstaPublicando() => _running;


        public MQContingencyService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _contingencyPath = $"{Directory.GetCurrentDirectory()}\\Contingency\\MessagesMQ\\";
        }

        public async Task StartAsync()
        {
            lock (_lockStartAsync)
            {
                if (_running)
                    return;

                _running = true;
            }

            if (!Directory.Exists(_contingencyPath))
                Directory.CreateDirectory(_contingencyPath);

            DirectoryInfo di = new DirectoryInfo(_contingencyPath);
            List<string> archivos = di.GetFiles()
                                        .OrderBy(x => x.CreationTimeUtc)
                                        .Select(x => x.FullName)
                                        .ToList();

            var mqService = new MQService(_serviceProvider);
            foreach (string archivo in archivos)
            {
                var content = await File.ReadAllTextAsync(archivo);
                var contingency = JsonConvert.DeserializeObject<ContingencyMQ>(content);
                await mqService.PublishAsync(contingency.MessageMQ, contingency.QueueMQ);    //SI FALLA VUELVE A CREAR OTRO ARCHIVO AL FINAL DE LA COLA CON MISMO MENSAJE
                File.Delete(archivo);
                if (!_running) break;   //SI SE SOLICITA EL Stop
            }

            _running = false;
        }

        public void Stop()
        {
            _running = false;
        }
    }
}
