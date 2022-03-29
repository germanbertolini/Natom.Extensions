using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Entities
{
    public class ContingencyMQ
    {
        public QueueMQ QueueMQ { get; set; }
        public MessageMQ MessageMQ { get; set; }
    }
}
