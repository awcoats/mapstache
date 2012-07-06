using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.SqlServer.Types;

namespace Mapstache
{
    public class GraphicsPathBuilder
    {
        private readonly float _metersToPixel;
        private readonly Point _topLeft;
        private readonly Size _size;

        public GraphicsPathBuilder(RectangleF bounds, Size size)
        {
            if (bounds.IsEmpty)
            {
                throw new ArgumentException("Bounds is empty.", "bounds");
            }
            if (size.IsEmpty)
            {
                throw new ArgumentException("Size is empty.", "size");
            }
            _metersToPixel = bounds.Width/size.Width;
            _topLeft = new Point((int) bounds.Left, (int) (bounds.Top));
            _size = size;
        }

        public GraphicsPath Build(SqlGeography geography)
        {

            var graphicsPath = new GraphicsPath();
            graphicsPath.FillMode = FillMode.Alternate;
            var geometryType = geography.STGeometryType();
            if (geometryType == "Polygon")
            {
                AddPolygon(geography, graphicsPath);
            }
            else if (geometryType == "MultiPolygon")
            {
                AddMultiPolygon(geography, graphicsPath);
            }
            else if (geometryType == "GeometryCollection")
            {
                for (int i = 0; i < geography.STNumGeometries(); i++)
                {
                    var geom = geography.STGeometryN(i + 1);
                    if (geom.STGeometryType().Value == "Polygon")
                    {
                        AddPolygon(geom, graphicsPath);
                    }
                    else if (geom.STGeometryType().Value == "MultiPolygon")
                    {
                        AddMultiPolygon(geom, graphicsPath);
                    }
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("The geometry type {0} is not supported.", geometryType));
            }
            return graphicsPath;
        }

        private void AddMultiPolygon(SqlGeography multiPolygon, GraphicsPath graphicsPath)
        {
            for (int i = 0; i < multiPolygon.STNumGeometries(); i++)
            {
                var geom = multiPolygon.STGeometryN(i + 1);
                AddPolygon(geom, graphicsPath);
            }
        }

        private void AddPolygon(SqlGeography geography, GraphicsPath graphicsPath)
        {
            for (int r = 0; r < geography.NumRings(); r++)
            {
                var coords = GetCoordinates(geography, r);
                graphicsPath.AddPolygon(coords.ToArray());
            }
        }

        private List<PointF> GetCoordinates(SqlGeography lineString, int r)
        {
            var ring = lineString.RingN(r + 1);
            var coords = new List<PointF>();
            for (int j = 0; j < ring.STNumPoints(); j++)
            {
                var point = ring.STPointN(j + 1);
                var ll = new PointF((float) point.Long, (float) point.Lat);
                var pixelPoint = GetPixel(ll);
                coords.Add(pixelPoint);
            }
            return coords;
        }

        private PointF GetPixel(PointF ll)
        {
            var meters = SphericalMercator.FromLonLat(ll);
            var x = (meters.X - _topLeft.X)/_metersToPixel;
            var y = (meters.Y - _topLeft.Y)/_metersToPixel;
            y = _size.Height - y;
            return new PointF(x, y);
        }
    }
}
