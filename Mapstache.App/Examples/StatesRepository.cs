using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;

namespace Utf8GridApplication.Examples
{
    public class StatesRepository
    {

        private SqlConnection CreateAndOpenConnection()
        {
            var connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["geoDatabase"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public IEnumerable<SqlDataReader> Query(SqlGeography bounds)
        {
            if (bounds.STIsEmpty())
            {
                yield break;
            }
            var query = "Select Geom,STATE_NAME,POP2000 From States where [Geom].Filter(@Geography) = 1";

            using (var connection = CreateAndOpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("Geography", bounds.STAsText());
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return reader;
                    }
                }
            }
        }

        public IEnumerable<SqlDataReader> Query(string query)
        {
            using (var connection = CreateAndOpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        yield return reader;
                    }
                }
            }
        }
    }
}
