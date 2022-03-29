using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Entities.Models
{
    [Table("Token")]
    public class Token
    {
        [ExplicitKey]
        public string Key { get; set; }

        [ExplicitKey] 
        public string Scope { get; set; }

        public string SyncInstanceId { get; set; }

        public int? UserId { get; set; }
        public string UserFullName { get; set; }

        public int? ClientId { get; set; }
        public string ClientFullName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
