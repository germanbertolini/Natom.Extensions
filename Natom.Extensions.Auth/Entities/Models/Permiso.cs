using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Entities.Models
{
    public class Permiso
    {
        public string PermisoId { get; set; }
        public string Scope { get; set; }
        public string Descripcion { get; set; }
    }
}
