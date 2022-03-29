using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Logger.Entities
{
    public class Transaction
    {
        public string TraceTransactionId { get; set; }
        public string IP { get; set; }
        public long? UserId { get; set; }
        public string UrlRequested { get; set; }
        public string ActionRequested { get; set; }
        public DateTime DateTime { get; set; }
        public string OS { get; set; }
        public string AppVersion { get; set; }
        public string Lang { get; set; }

        public string Scope { get; set; }

        public string InstanceId { get; set; }

        public string HostName { get; set; }
        public int? Port { get; set; }
    }
}
