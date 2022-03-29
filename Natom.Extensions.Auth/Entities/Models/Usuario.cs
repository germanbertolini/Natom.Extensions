using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Entities.Models
{
	[Table("Usuario")]
	public class Usuario
    {
		[Key]
		public int UsuarioId { get; set; }

		public string Scope { get; set; }
		public string Nombre { get; set; }
		public string Apellido { get; set; }
		public string Email { get; set; }
		public string Clave { get; set; }
		public DateTime? FechaHoraConfirmacionEmail { get; set; }
		public DateTime? FechaHoraUltimoEmailEnviado { get; set; }
		public string SecretConfirmacion { get; set; }
		public DateTime FechaHoraAlta { get; set; }
		public DateTime? FechaHoraBaja { get; set; }

		public int? ClienteId { get; set; }
		public string ClienteNombre { get; set; }

		[Write(false)]
		public List<UsuarioPermiso> Permisos { get; set; }
	}
}
