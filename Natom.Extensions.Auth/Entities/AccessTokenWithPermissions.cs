using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Entities
{
    public class AccessTokenWithPermissions : AccessToken
    {
        public List<string> Permissions { get; set; }
    }
}
