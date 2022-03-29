using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Logger.PackageConfig
{
    public class LoggerServiceConfig
    {
        public int InsertEachMS { get; set; }
        public int BulkInsertSize { get; set; }
        public string SystemName { get; set; }
    }
}
