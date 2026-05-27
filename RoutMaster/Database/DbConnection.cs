using System.Data;
using Npgsql;

namespace RouteMaster.Database
{
    public class DbConnection
    {
        private static string _connectionString = "Host=localhost;Port=5432;Database=RouteMaster;Username=postgres;Password=1111";

        public static IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}