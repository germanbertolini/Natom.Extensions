using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Entities
{
    public class QueueMQ
    {
        public string QueueName { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
    }
}
