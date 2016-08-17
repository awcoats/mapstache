using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;

namespace MapStache.Web.Examples
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
            var query = $"Select Shape,Name From sde.COUNTY_ESRI_WGS84 where [Shape].Filter( geography::STGeomFromText(@Geography,{EPSG.WGS84})) = 1 and STATE_FIPS!='02'";

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
