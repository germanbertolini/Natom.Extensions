using Dapper;
using Dapper.Contrib.Extensions;
using Natom.Extensions.Auth.Entities.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Repository
{
    public class TokenRepository : BaseRepository
    {
        public TokenRepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        public async Task AddAsync(Token token)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.InsertAsync(token);
            }
        }

        public async Task RemoveAsync(Token token)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.DeleteAsync(token);
            }
        }

        public async Task<List<Token>> GetAllAsync()
        {
            List<Token> result = new List<Token>();
            using (var db = new SqlConnection(_connectionString))
            {
                var enumerable = await db.GetAllAsync<Token>();
                result = enumerable.ToList();
            }
            return result;
        }

        public async Task<Token> GetTokenByUserAndScopeAsync(int? userId, string scope)
        {
            Token token = null;
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_token_select_by_userid_and_scope] @UserId, @Scope";
                var _params = new { UserId = userId, Scope = scope };
                token = (await db.QueryAsync<Token>(sql, _params)).FirstOrDefault();
            }
            return token;
        }

        public async Task<List<Token>> ListTokensByClientAndScopeAsync(int clientId, string scope)
        {
            List<Token> tokens = new List<Token>();
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_token_select_by_clientid_and_scope] @ClientId, @Scope";
                var _params = new { ClientId = clientId, Scope = scope };
                tokens = (await db.QueryAsync<Token>(sql, _params)).ToList();
            }
            return tokens;
        }

        public async Task<List<Token>> ListTokensByUsuarioIdAsync(int usuarioId)
        {
            List<Token> tokens = new List<Token>();
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_token_select_by_usuarioid] @UsuarioId";
                var _params = new { UsuarioId = usuarioId };
                tokens = (await db.QueryAsync<Token>(sql, _params)).ToList();
            }
            return tokens;
        }

        public async Task DeleteTokensByUsuarioIdAsync(int usuarioId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_token_delete_by_usuarioid] @UsuarioId";
                var _params = new { UsuarioId = usuarioId };
                await db.ExecuteAsync(sql, _params);
            }
        }

        public async Task DeleteTokensByClientAndScopeAsync(int clientId, string scope)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_token_delete_by_clientid_and_scope] @ClientId, @Scope";
                var _params = new { ClientId = clientId, Scope = scope };
                await db.ExecuteAsync(sql, _params);
            }
        }

        public async Task DeleteTokenAsync(string tokenKey, string scope)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "EXEC [dbo].[sp_token_delete] @TokenKey, @Scope";
                var _params = new { TokenKey = tokenKey, Scope = scope };
                await db.ExecuteAsync(sql, _params);
            }
        }
    }
}
