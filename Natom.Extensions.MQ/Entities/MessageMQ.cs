using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Entities
{
    public class MessageMQ
    {
        public string Topic { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string Message { get; set; }
        public ProducerInfoMQ ProducerInfo { get; set; }
    }
}
