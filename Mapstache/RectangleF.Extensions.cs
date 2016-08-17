using System.Data.SqlTypes;
using System.Drawing;
using Microsoft.SqlServer.Types;

namespace MapStache
{
    public static class RectangleFExtensions
    {
        public static SqlGeography ToSqlGeography(RectangleF bbox)
        {
            // select geography::STGeomFromText('POLYGON ((-146.3835 33.51345, -63.32683 33.51345, -63.32683 65.84304, -146.3835 65.84304, -146.3835 33.51345))', 4326)
            // select geography::STGeomFromText('POLYGON ((-140.5827 26.51924,-99.05437 26.51924,-57.52604 26.51924,-57.52604 62.31364,-99.05437 62.31364,-140.5827 62.31364,-140.5827 26.51924))', 4326)
            var halfWidth = (int)bbox.Width / 2;

            //TODO - add 4 points along along the top and bottom lines to try and get a 'square'
            var point1 = new System.Drawing.Point((int)bbox.Left, (int)bbox.Top);
            var point1b = new System.Drawing.Point((int)bbox.Left + halfWidth, (int)bbox.Top);
            var point2 = new System.Drawing.Point((int)bbox.Right, (int)bbox.Top);
            var point3 = new System.Drawing.Point((int)bbox.Right, (int)bbox.Bottom);
            var point3b = new System.Drawing.Point((int)bbox.Right - halfWidth, (int)bbox.Bottom);
            var point4 = new System.Drawing.Point((int)bbox.Left, (int)bbox.Bottom);
            var point1ll = SphericalMercator.ToLonLat(point1);
            var point1bll = SphericalMercator.ToLonLat(point1b);
            var point2ll = SphericalMercator.ToLonLat(point2);
            var point3ll = SphericalMercator.ToLonLat(point3);
            var point3bll = SphericalMercator.ToLonLat(point3b);
            var point4ll = SphericalMercator.ToLonLat(point4);
            var wkt = string.Format("POLYGON (({0} {1},{2} {3},{4} {5},{6} {7},{8} {9},{10} {11},{12} {13}))",
                point1ll.X, point1ll.Y, point1bll.X, point1bll.Y, point2ll.X, point2ll.Y, point3ll.X, point3ll.Y, point3bll.X, point3bll.Y, point4ll.X, point4ll.Y,
                point1ll.X, point1ll.Y);

            return SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4269);
        }
    }
}
