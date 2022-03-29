using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Attributes
{
    public class TienePermisoAttribute : Attribute
    {
        public string Permiso { get; set; }
    }
}
