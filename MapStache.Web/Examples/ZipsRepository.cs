using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;

namespace MapStache.Web.Examples
{
    public class ZipsRepository
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
            var query = "Select Geom,Name as Zip,'aaa' as PO_NAME From US_COUNTY_2015 where [Geom].Filter( geography::STGeomFromText(@Geography,4269)) = 1 and STATEFP!='02'";

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
