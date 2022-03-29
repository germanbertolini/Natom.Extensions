using Dapper;
using Microsoft.Extensions.Configuration;
using Natom.Extensions.Configuration.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Configuration.Repository
{
    public class ConfigRepository
    {
        private string _connectionString;

        public ConfigRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Config>> GetConfigAsync()
        {
            var config = new List<Config>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<Config>("SELECT * FROM Config");
                config = result.ToList();
            }

            return config;
        }
    }
}
