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

            //TODO - make sure box is no bigger than -180,-90 to 360,90

            //bbox = SphericalMercator.ToLonLat(bbox);
            var tlLatLng = new PointF(bbox.Left, bbox.Top);
            var brLatLng = new PointF(bbox.Right, bbox.Bottom);

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
