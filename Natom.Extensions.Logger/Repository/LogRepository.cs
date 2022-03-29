using Dapper;
using Microsoft.Extensions.Configuration;
using Natom.Extensions.Configuration.Entities;
using Natom.Extensions.Logger.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.Logger.Repository
{
    public class LogRepository
    {
        private string _connectionString;

        public LogRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Transaction>> GetConfigAsync()
        {
            var config = new List<Transaction>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<Transaction>("SELECT * FROM Config");
                config = result.ToList();
            }

            return config;
        }

        public async Task BulkInsertAsync(List<Transaction> transactions)
        {
            var sql = "INSERT INTO[dbo].[Transaction] " +
                       "    ([TraceTransactionId] " +
                       "        ,[IP] " +
                       "        ,[UserId] " +
                       "        ,[UrlRequested] " +
                       "        ,[ActionRequested] " +
                       "        ,[DateTime] " +
                       "        ,[OS] " +
                       "        ,[AppVersion] " +
                       "        ,[Lang] " +
                       "        ,[Scope] " +
                       "        ,[InstanceId] " +
                       "        ,[HostName] " +
                       "        ,[Port]) " +
                       "    VALUES " +
                       "    (@TraceTransactionId " +
                       "        ,@IP " +
                       "        ,@UserId " +
                       "        ,@UrlRequested " +
                       "        ,@ActionRequested " +
                       "        ,@DateTime " +
                       "        ,@OS " +
                       "        ,@AppVersion " +
                       "        ,@Lang " +
                       "        ,@Scope " +
                       "        ,@InstanceId " +
                       "        ,@HostName " +
                       "        ,@Port); ";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, transactions);
            }
        }

        public async Task BulkInsertAsync(List<TransactionLog> transactionLogs)
        {
            var sql = "INSERT INTO[dbo].[TransactionLog] " +
                    "   ([TraceTransactionId] " +
                    "       ,[DateTime] " +
                    "       ,[Type] " +
                    "       ,[Description] " +
                    "       ,[Data]) " +
                    "VALUES " +
                    "   (@TraceTransactionId " +
                    "       ,@DateTime " +
                    "       ,@Type " +
                    "       ,@Description " +
                    "       ,@Data);  ";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql, transactionLogs);
            }
        }
    }
}
