﻿using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;

namespace Mapstache
{
    public class GeometryDataSource
    {
        private SqlConnection CreateAndOpenConnection()
        {
            var connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["geoDatabase"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public IEnumerable<SqlDataReader> Query(SqlGeography bounds, string layer)
        {
            if (bounds.STIsEmpty())
            {
                yield break;
            }
            var query = string.Format("Select Geom,STATE_NAME,POP2000 From {0} where [Geom].Filter(@Geography) = 1", layer);

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
