using Dapper;
using Dapper.Contrib.Extensions;
using Natom.Extensions.Common.Exceptions;
using Natom.Extensions.Auth.Entities.Models;
using Natom.Extensions.Auth.Entities.Results;
using Natom.Extensions.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Repository
{
    public class UsuarioRepository : BaseRepository
    {
        public UsuarioRepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        public async Task AddAsync(Usuario usuario)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.InsertAsync(usuario);
            }
        }

        public async Task<Usuario> GetByEmailAndScopeAsync(string email, string scope)
        {
            Usuario usuario = null;
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_select_by_email_and_scope] @Email, @Scope";
                using (var results = db.QueryMultiple(sql, new { Email = email, Scope = scope }))
                {
                    usuario = (await results.ReadAsync<Usuario>()).ToList().FirstOrDefault();
                    if (usuario != null)
                        usuario.Permisos = (await results.ReadAsync<UsuarioPermiso>()).ToList();
                }
            }
            return usuario;
        }

        public async Task<List<spUsuariosListByClienteAndScopeResult>> ListByClienteAndScopeAsync(string scope, string search, int skip, int take, int clienteId = 0)
        {
            List<spUsuariosListByClienteAndScopeResult> usuarios = null;
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_list_by_cliente_and_scope] @Scope, @ClienteId, @Search, @Skip, @Take";
                var _params = new { Scope = scope, ClienteId = clienteId, Search = search, Skip = skip, Take = take };
                usuarios = (await db.QueryAsync<spUsuariosListByClienteAndScopeResult>(sql, _params)).ToList();
            }
            return usuarios;
        }

        public async Task<List<Permiso>> ListPermisosAsync(string scope)
        {
            List<Permiso> permisos = null;
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_permisos_select_by_scope] @Scope";
                var _params = new { Scope = scope };
                permisos = (await db.QueryAsync<Permiso>(sql, _params)).ToList();
            }
            return permisos;
        }

        public async Task<Usuario> ObtenerUsuarioAsync(int usuarioId)
        {
            Usuario usuario = null;
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_select_by_id] @UsuarioId";
                using (var results = db.QueryMultiple(sql, new { UsuarioId = usuarioId }))
                {
                    usuario = (await results.ReadAsync<Usuario>()).ToList().FirstOrDefault();
                    if (usuario != null)
                        usuario.Permisos = (await results.ReadAsync<UsuarioPermiso>()).ToList();
                }
            }
            return usuario;
        }

        public async Task EliminarUsuarioAsync(int usuarioId, int bajaByUserId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_baja] @UsuarioId, @BajaByUsuarioId";
                var _params = new { UsuarioId = usuarioId, BajaByUsuarioId = bajaByUserId };
                await db.ExecuteAsync(sql, _params);
            }
        }

        public async Task ConfirmarUsuarioAsync(string secret, string claveMD5)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_confirmar] @Secret, @ClaveMD5";
                var _params = new { Secret = secret, ClaveMD5 = claveMD5 };
                var returnValue = (await db.ExecuteAsync(sql, _params));
                if (returnValue == 0)
                    throw new HandledException("El usuario ya ha confirmado su email.");
            }
        }

        public Task<Usuario> GuardarUsuarioAsync(string scope, Usuario user, string secretConfirmation, int byUserId)
        {
            if (user.UsuarioId == 0)
                return GuardarNewUsuarioAsync(scope, user, secretConfirmation, byUserId);
            else
                return GuardarUpdateUsuarioAsync(scope, user, byUserId);
        }

        private async Task<Usuario> GuardarUpdateUsuarioAsync(string scope, Usuario user, int byUserId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_update] @UsuarioId, @Scope, @Nombre, @Apellido, @PermisosId, @UpdatedByUsuarioId";
                var _params = new { UsuarioId = user.UsuarioId, Scope = scope, Nombre = user.Nombre, Apellido = user.Apellido,
                                    PermisosId = user.Permisos.Select(p => new { ID = p.PermisoId }).AsTableValuedParameter("dbo.ID_char50_list", new[] { "ID" }),
                                    UpdatedByUsuarioId = byUserId};
                var returnValue = (await db.ExecuteAsync(sql, _params));
                if (returnValue == -1)
                    throw new HandledException("El usuario no existe.");
            }
            return user;
        }

        public async Task<List<Usuario>> ObtenerUsuariosPorIdsAsync(List<int> usuariosIds)
        {
            var usuarios = new List<Usuario>();
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_select_by_multiples_ids] @UsuariosIds";
                var _params = new { UsuariosIds = usuariosIds.Select(p => new { ID = p }).AsTableValuedParameter("dbo.ID_int_list", new[] { "ID" }) };
                usuarios = (await db.QueryAsync<Usuario>(sql, _params)).ToList();
            }
            return usuarios;
        }

        private async Task<Usuario> GuardarNewUsuarioAsync(string scope, Usuario user, string secretConfirmation, int byUserId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_create] @Scope, @Nombre, @Apellido, @Email, @SecretConfirmation, @ClienteId, @PermisosId, @CreatedByUsuarioId";
                var _params = new { Scope = scope, Nombre = user.Nombre, Apellido = user.Apellido, Email = user.Email,
                                    SecretConfirmation = secretConfirmation, ClienteId = (user.ClienteId ?? 0),
                                    PermisosId = user.Permisos.Select(p => new { ID = p.PermisoId }).AsTableValuedParameter("dbo.ID_char50_list", new[] { "ID" }),
                                    CreatedByUsuarioId = byUserId };
                var returnValue = (await db.ExecuteAsync(sql, _params));
                if (returnValue == -1)
                    throw new HandledException("El usuario ya existe.");
                else
                {
                    user.UsuarioId = returnValue;
                    user.SecretConfirmacion = secretConfirmation;
                }
            }
            return user;
        }
        
        public async Task<Usuario> RecuperarUsuarioByEmailAsync(string scope, string email, string secretConfirmation, int byUserId)
        {
            Usuario usuario = null;
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_recover_by_email] @Scope, @Email, @SecretConfirmation, @ByUsuarioId";
                var _params = new
                {
                    Scope = scope,
                    Email = email,
                    SecretConfirmation = secretConfirmation,
                    ByUsuarioId = byUserId
                };
                usuario = (await db.QueryAsync<Usuario>(sql, _params)).FirstOrDefault();
                if (usuario == null)
                    throw new HandledException("El usuario no existe.");
            }
            return usuario;
        }

        public async Task<Usuario> RecuperarUsuarioAsync(string scope, int usuarioId, string secretConfirmation, int byUserId)
        {
            Usuario usuario = null;
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_usuarios_recover_by_id] @Scope, @UsuarioId, @SecretConfirmation, @ByUsuarioId";
                var _params = new
                {
                    Scope = scope,
                    UsuarioId = usuarioId,
                    SecretConfirmation = secretConfirmation,
                    ByUsuarioId = byUserId
                };
                usuario = (await db.QueryAsync<Usuario>(sql, _params)).FirstOrDefault();
                if (usuario == null)
                    throw new HandledException("El usuario no existe.");
            }
            return usuario;
        }
    }
}
