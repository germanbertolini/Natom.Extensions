using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Entities.Results
{
    public class spUsuariosListByClienteAndScopeResult
    {
        public int UsuarioId { get; set; }
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Estado { get; set; }
        public DateTime FechaHoraAlta { get; set; }
        public string ClienteRazonSocial { get; set; }
        public string ClienteCUIT { get; set; }
        public string Rol { get; set; }

        public int TotalFiltrados { get; set; }
        public int TotalRegistros { get; set; }
    }
}
