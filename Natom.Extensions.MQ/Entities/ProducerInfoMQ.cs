using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Entities
{
    public class ProducerInfoMQ
    {
        public int ClientId { get; set; }
        public string SyncInstanceId { get; set; }
    }
}
