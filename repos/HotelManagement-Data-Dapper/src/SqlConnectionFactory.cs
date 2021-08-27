using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace HotelManagement_Data_Dapper.src
{
    /// <summary>
    /// Sql connection factory
    /// </summary>
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string connectionString;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlConnectionFactory(string connectionString)
        {
            //Verify.NotEmpty(nameof(connectionString), connectionString);
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Get the IDbConnection
        /// </summary>
        /// <returns></returns>
        public async Task<SqlConnection> GetConnection()
        {
            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                await connection.OpenAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message+"SQL availability", ex);
            }

            return connection;
        }
    }
}
