using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace MapStache
{
    public static class RectangleFExtensions
    {
        public static SqlGeography ToSqlGeography(this RectangleF bbox)
        {
            var wkt = ToUnprojectedWkt(bbox);

            return SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4269);
        }

        public static string ToUnprojectedWkt(this RectangleF bbox)
        {
            // select geography::STGeomFromText('POLYGON ((-146.3835 33.51345, -63.32683 33.51345, -63.32683 65.84304, -146.3835 65.84304, -146.3835 33.51345))', 4326)
            // select geography::STGeomFromText('POLYGON ((-140.5827 26.51924,-99.05437 26.51924,-57.52604 26.51924,-57.52604 62.31364,-99.05437 62.31364,-140.5827 62.31364,-140.5827 26.51924))', 4326)
            var segment = (int)bbox.Width / 10;

            //TODO - add 4 points along along the top and bottom lines to try and get a 'square'
            var points = new List<Point>();
            points.Add(new Point((int) bbox.Left, (int) bbox.Top));
            for (int i = 0; i < 10; i++)
            {
                points.Add(new Point((int)bbox.Left + (segment*i), (int)bbox.Top));
            }
          
            points.Add(new Point((int) bbox.Right, (int) bbox.Top));
            points.Add(new Point((int) bbox.Right, (int) bbox.Bottom));

            for (int i = 0; i < 10; i++)
            {
                points.Add(new Point((int)bbox.Left + (segment *(10-i)), (int)bbox.Bottom));
            }

            //points.Add(new Point((int) bbox.Right - halfWidth, (int) bbox.Bottom));
            points.Add(new Point((int) bbox.Left, (int) bbox.Bottom));

            var unprojectedPoints = points.Select(SphericalMercator.ToLonLat).ToList();

            var wkt = new StringBuilder();
            wkt.Append("POLYGON ((");
            for (int i = 0; i < unprojectedPoints.Count; i++)
            {
                wkt.Append($"{unprojectedPoints[i].X} {unprojectedPoints[i].Y},");
            }
            wkt.Append($"{unprojectedPoints[0].X} {unprojectedPoints[0].Y}");
            wkt.Append("))");

            return wkt.ToString();
        }
    }
}
