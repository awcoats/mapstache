using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Mapstache
{
    // lots of new comments on this branch
    public static partial class SqlGeographyExtensions
    {
        private class ProjectSink : IGeographySink110
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
                _builder.SetSrid(srid);
            }

            public SqlGeometry ConstructedGeometry
            {
                get { return _builder.ConstructedGeometry; }
            }

            public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
            {
                // lets try and make a pull request.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Projects a SqlGeography to transverse mercator.
        /// </summary>
        /// <param name="geography"></param>
        /// <returns></returns>
        public static SqlGeometry FromLonLat(this SqlGeography geography)
        {
            var projectionSink = new ProjectSink();
            geography.Populate(projectionSink);
            return projectionSink.ConstructedGeometry;
        }
    }
}
