using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Mapstache
{
    public static partial class SqlGeographyExtensions
    {
        private class ProjectSink : IGeographySink
        {
            private readonly SqlGeometryBuilder _builder = new SqlGeometryBuilder();

            public void AddLine(double latitude, double longitude, double? z, double? m)
            {
                double x = 0;
                double y = 0;
                SphericalMercator.FromLonLat(longitude, latitude, out x, out y);
                _builder.AddLine(x, y, z, m);
            }

            public void BeginFigure(double latitude, double longitude, double? z, double? m)
            {
                double x = 0;
                double y = 0;
                SphericalMercator.FromLonLat(longitude, latitude, out x, out y);
                _builder.BeginFigure(x, y, z, m);
            }

            public void BeginGeography(OpenGisGeographyType type)
            {
                _builder.BeginGeometry((OpenGisGeometryType)type);
            }

            public void EndFigure()
            {
                _builder.EndFigure();
            }

            public void EndGeography()
            {
                _builder.EndGeometry();
            }

            public void SetSrid(int srid)
            {
                _builder.SetSrid(EPSG.SphericalMercator);
            }

            public SqlGeometry ConstructedGeometry
            {
                get { return _builder.ConstructedGeometry; }
            }
        }

        public static SqlGeometry FromLonLat(this SqlGeography geography)
        {
            var projectionSink = new ProjectSink();
            geography.Populate(projectionSink);
            return projectionSink.ConstructedGeometry;
        }
    }
}
