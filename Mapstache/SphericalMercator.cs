using System;
using System.Drawing;

namespace MapStache
{
    public static class SphericalMercator
    {
        private readonly static double radius = 6378137;
        private static double D2R = Math.PI / 180;
        private static double HALF_PI = Math.PI / 2;

        public static Point FromLonLat(PointF lonlat)
        {
            double lon = lonlat.X;
            double lat = lonlat.Y;
            double lonRadians = (D2R * lon);
            double latRadians = (D2R * lat);

            double x = radius * lonRadians;
            double y = radius * Math.Log(Math.Tan(Math.PI * 0.25 + latRadians * 0.5));

            return new Point((int)x, (int)y);
        }

        public static void FromLonLat(double lon, double lat, out double x, out double y)
        {
            double lonRadians = (D2R * lon);
            double latRadians = (D2R * lat);

            x = radius * lonRadians;
            y = radius * Math.Log(Math.Tan(Math.PI * 0.25 + latRadians * 0.5));
        }

        public static void ToLonLat(double x, double y, out double lon, out double lat)
        {
            double ts = Math.Exp(-y / (radius));
            double latRadians = HALF_PI - 2 * Math.Atan(ts);

            double lonRadians = x / (radius);

            lon = (lonRadians / D2R);
            lat = (latRadians / D2R);
        }

        public static PointF ToLonLat(Point xy)
        {
            double x = xy.X;
            double y = xy.Y;
            double ts = Math.Exp(-y / (radius));
            double latRadians = HALF_PI - 2 * Math.Atan(ts);

            double lonRadians = x / (radius);

            double lon = (lonRadians / D2R);
            double lat = (latRadians / D2R);

            return new PointF((float)lon, (float)lat);
        }

        public static Rectangle FromLonLat(RectangleF bbox)
        {
            var min = FromLonLat(new PointF(bbox.X, bbox.Y));
            var max = FromLonLat(new PointF(bbox.X + bbox.Width, bbox.Y + bbox.Height));
            return Rectangle.FromLTRB(min.X, min.Y, max.X, max.Y);
        }

        public static RectangleF ToLonLat(RectangleF bbox)
        {
            var min = ToLonLat(new Point((int)bbox.X, (int)bbox.Y));
            var max = ToLonLat(new Point((int)(bbox.X + bbox.Width), (int)(bbox.Y + bbox.Height)));
            return RectangleF.FromLTRB(min.X, min.Y, max.X, max.Y);
        }
    }
}
