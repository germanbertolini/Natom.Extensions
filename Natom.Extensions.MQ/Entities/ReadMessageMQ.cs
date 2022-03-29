using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Entities
{
    public class ReadMessageMQ<TMessage>
    {
        public int ThreadNumber { get; set; }
        public int CycleNumber { get; set; }
        public TMessage Content { get; set; }
        public ulong DeliveryTag { get; set; }
        private bool _removeFromQueue = true;
        public void AbortRemovingFromQueue() => _removeFromQueue = false;
        public bool MustBeRemoved() => _removeFromQueue;
    }
}
