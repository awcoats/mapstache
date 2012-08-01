using System;
using System.Data.SqlTypes;
using System.Drawing;
using Microsoft.SqlServer.Types;

namespace Mapstache
{
    public static class RectangleFExtensions
    {
        public static SqlGeography ToSqlGeography(this RectangleF bbox)
        {
            if (bbox.Width == 0 && bbox.Height == 0)
            {
                return new SqlGeography();
            }


            //var tlLatLng = new PointF(bbox.Left, bbox.Top);
            //var brLatLng = new PointF(bbox.Right, bbox.Bottom);

            var tlLatLng = new PointF(Math.Max(-180, bbox.Left), Math.Max(0, bbox.Top));
            var brLatLng = new PointF(Math.Min(-1, bbox.Right), Math.Min(89, bbox.Bottom));

            //TODO: create geography using GeographyBuilder and then convert to WKT.
            var wkt = string.Format(
                    "POLYGON (({0} {1},{2} {3},{4} {5},{6} {7},{8} {9}))",
                    tlLatLng.X, tlLatLng.Y,
                    brLatLng.X, tlLatLng.Y,
                    brLatLng.X, brLatLng.Y,
                    tlLatLng.X, brLatLng.Y,
                    tlLatLng.X, tlLatLng.Y);
            return SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), EPSG.WGS84);
        }

        
    }
}
