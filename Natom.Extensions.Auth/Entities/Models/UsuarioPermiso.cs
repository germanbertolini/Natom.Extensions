using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Entities.Models
{
    [Table("UsuarioPermiso")]
    public class UsuarioPermiso
    {
        [Key]
        public int UsuarioPermisoId { get; set; }

	    public int UsuarioId { get; set; }
        public string PermisoId { get; set; }
    }
}
