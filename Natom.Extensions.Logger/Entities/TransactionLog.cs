using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Logger.Entities
{
    public class TransactionLog
    {
        public string TraceTransactionId { get; set; }
        public DateTime DateTime { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Data { get; set; }
    }
}
