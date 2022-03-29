using AutoMapper;
using Natom.Extensions.Common.Exceptions;
using Natom.Extensions.Auth.Entities;
using Natom.Extensions.Auth.Entities.Models;
using Natom.Extensions.Auth.Exceptions;
using Natom.Extensions.Auth.Helpers;
using Natom.Extensions.Auth.PackageConfig;
using Natom.Extensions.Auth.Repository;
using Natom.Extensions.Cache.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Auth.Services
{
    public class AuthService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CacheService _cacheService;
        private readonly AuthServiceConfig _config;
        private readonly Mapper _mapper;
        private readonly IDictionary<string, string> _endpointPermissions;

        public AuthService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cacheService = (CacheService)serviceProvider.GetService(typeof(CacheService));
            _mapper = (Mapper)serviceProvider.GetService(typeof(Mapper));
            _config = (AuthServiceConfig)serviceProvider.GetService(typeof(AuthServiceConfig));
            _endpointPermissions = new Dictionary<string, string>();
        }

        public async Task<Usuario> AuthenticateUserAsync(string email, string password)
        {
            string scope = _config.Scope;

            var repository = new UsuarioRepository(_serviceProvider);
            var usuario = await repository.GetByEmailAndScopeAsync(email, scope);

            if (usuario == null)
                throw new HandledException("El usuario no existe");

            if (usuario.FechaHoraBaja.HasValue)
                throw new HandledException("Usuario dado de baja");

            if (!string.IsNullOrEmpty(usuario.SecretConfirmacion) && usuario.FechaHoraConfirmacionEmail == null)
                throw new HandledException("Revise su casilla de correo electrónico para establecer la contraseña");

            if (!usuario.Clave.Equals(CreateMD5(password)))
                throw new HandledException("Usuario y/o clave incorrecta");

            return usuario;
        }

        private static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public void RegisterEndpointPermission(string endpoint, string permiso)
        {
            _endpointPermissions.Add(endpoint, permiso);
        }

        public bool EndpointTienePermiso(string endpoint, string permiso)
        {
            return permiso.Equals("*")
                ||
                (
                    !_endpointPermissions.ContainsKey(endpoint)
                        || (_endpointPermissions.ContainsKey(endpoint) && _endpointPermissions[endpoint].Contains(permiso))
                );
        }

        public async Task<AccessToken> CreateTokenAsync(int? userId, string userName, int? clientId, string clientName, List<string> permissions, long tokenDurationMinutes)
        {
            string scope = _config.Scope;
            var token = new Token
            {
                Key = Guid.NewGuid().ToString("N"),
                Scope = scope.Length > 20 ? scope.Substring(0, 20) : scope,
                UserId = userId,
                UserFullName = userName,
                ClientId = clientId,
                ClientFullName = clientName,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(tokenDurationMinutes)
            };

            var repository = new TokenRepository(_serviceProvider);
            await repository.AddAsync(token);

            await _cacheService.SetValueAsync($"Auth.Tokens.{token.Scope}.{token.Key}", JsonConvert.SerializeObject(permissions), TimeSpan.FromMinutes(tokenDurationMinutes));

            return new AccessToken
            {
                Key = token.Key,
                Scope = token.Scope,
                UserId = token.UserId,
                UserFullName = token.UserFullName,
                ClientId = token.ClientId,
                ClientFullName = token.ClientFullName,
                CreatedAt = token.CreatedAt,
                ExpiresAt = token.ExpiresAt
            };
        }

        public async Task DestroyTokenAsync(int userId, string scope = null)
        {
            if (string.IsNullOrEmpty(scope))
                scope = _config.Scope;

            scope = scope.Length > 20 ? scope.Substring(0, 20) : scope;

            var repository = new TokenRepository(_serviceProvider);
            var token = await repository.GetTokenByUserAndScopeAsync(userId, scope);

            await this.DestroyTokenAsync(token);
        }

        public async Task DestroyTokenAsync(Token token)
        {
            if (token != null)
            {
                var repository = new TokenRepository(_serviceProvider);
                await repository.DeleteTokenAsync(token.Key, token.Scope);
                await _cacheService.RemoveAsync($"Auth.Tokens.{token.Scope}.{token.Key}");
            }
        }

        public async Task DestroyTokensByScopeAndClientIdAsync(string scope, int clientId)
        {
            var repository = new TokenRepository(_serviceProvider);
            var tokens = await repository.ListTokensByClientAndScopeAsync(clientId, scope);

            foreach (var token in tokens)
            {
                await _cacheService.RemoveAsync($"Auth.Tokens.{token.Scope}.{token.Key}");
            }

            await repository.DeleteTokensByClientAndScopeAsync(clientId, scope);
        }

        public async Task DestroyTokensByUsuarioIdAsync(int usuarioId)
        {
            var repository = new TokenRepository(_serviceProvider);
            var tokens = await repository.ListTokensByUsuarioIdAsync(usuarioId);

            foreach (var token in tokens)
            {
                await _cacheService.RemoveAsync($"Auth.Tokens.{token.Scope}.{token.Key}");
            }

            await repository.DeleteTokensByUsuarioIdAsync(usuarioId);
        }

        public async Task<AccessToken> CreateTokenForSynchronizerAsync(string instanceId, string userName, int? clientId, string clientName, List<string> permissions, long tokenDurationMinutes)
        {
            var scope = _config.Scope;
            var token = new Token
            {
                Key = Guid.NewGuid().ToString("N"),
                Scope = scope.Length > 20 ? scope.Substring(0, 20) : scope,
                SyncInstanceId = instanceId,
                UserFullName = userName,
                ClientId = clientId,
                ClientFullName = clientName,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(tokenDurationMinutes)
            };

            var repository = new TokenRepository(_serviceProvider);
            await repository.AddAsync(token);

            await _cacheService.SetValueAsync($"Auth.Tokens.{token.Scope}.{token.Key}", JsonConvert.SerializeObject(permissions), TimeSpan.FromMinutes(tokenDurationMinutes));

            return new AccessToken
            {
                Key = token.Key,
                Scope = token.Scope,
                SyncInstanceId = token.SyncInstanceId,
                UserFullName = token.UserFullName,
                ClientId = token.ClientId,
                ClientFullName = token.ClientFullName,
                CreatedAt = token.CreatedAt,
                ExpiresAt = token.ExpiresAt
            };
        }

        public AccessToken DecodeToken(AccessToken injectedAccessToken, string bearerToken)
        {
            AccessToken accessToken = null;

            if (string.IsNullOrEmpty(bearerToken))
                throw new InvalidTokenException("Token inválido.");

            var stringToken = bearerToken.Replace("Bearer ", string.Empty);
            accessToken = OAuthHelper.Decode(stringToken);

            _mapper.Map(accessToken, injectedAccessToken);

            return accessToken;
        }

        public async Task<AccessTokenWithPermissions> DecodeAndValidateTokenAsync(AccessToken injectedAccessToken, string bearerToken)
        {
            var accessTokenWithPermissions = await DecodeAndValidateTokenAsync(bearerToken);
            _mapper.Map(accessTokenWithPermissions, injectedAccessToken);
            return accessTokenWithPermissions;
        }

        public async Task<AccessTokenWithPermissions> DecodeAndValidateTokenAsync(string bearerToken)
        {
            AccessToken accessToken = null;

            if (string.IsNullOrEmpty(bearerToken))
                throw new InvalidTokenException("Token inválido.");

            try
            {
                var stringToken = bearerToken.Replace("Bearer ", string.Empty);
                accessToken = OAuthHelper.Decode(stringToken);
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException("Formato token inválido.");
            }

            if (accessToken.ExpiresAt.HasValue && accessToken.ExpiresAt.Value < DateTime.Now)
                throw new InvalidTokenException("Token vencido.");

            var tokenCache = await _cacheService.GetValueAsync($"Auth.Tokens.{accessToken.Scope}.{accessToken.Key}");
            if (string.IsNullOrEmpty(tokenCache))
                throw new InvalidTokenException("Token vencido.");

            var accessTokenWithPermissions = _mapper.Map<AccessTokenWithPermissions>(accessToken);
            accessTokenWithPermissions.Permissions = JsonConvert.DeserializeObject<List<string>>(tokenCache);

            return accessTokenWithPermissions;
        }
    }
}
