using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace HotelManagement_Data_Dapper.src
{
    /// <summary>
    /// Interface for IDbConnectionFactory
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Get SqlConnection
        /// </summary>
        /// <returns></returns>
        Task<SqlConnection> GetConnection();
    }
}
