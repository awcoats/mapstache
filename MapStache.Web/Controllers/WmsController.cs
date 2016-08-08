using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Mapstache;
using Microsoft.SqlServer.Types;

namespace Utf8GridApplication.Controllers
{
    public class WmsController : Controller
    {
        public ActionResult Index(int width, int height, string bbox, string layers)
        {
            var bounds = CreateBBox(bbox);
            var boundsLL = SphericalMercator.ToLonLat(bounds);
            var boundsGeographyLL = boundsLL.ToSqlGeography();
            var  states = new GeometryDataSource().Query(boundsGeographyLL, "US_COUNTY_2015");;

            var memoryStream = new MemoryStream();
            using (var bitmap = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bitmap))
            using (var pen = new Pen(Color.Blue,2.0f))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                foreach (var state in states)
                {
                    var geography = (SqlGeography)state["Geom"];
                    {
                        using (var gp = CreateGraphicsPath(bounds, bitmap, geography))
                        {
                            g.DrawPath(pen, gp);
                        }
                    }
                }
                bitmap.Save(memoryStream, ImageFormat.Png);
            }
            return File(memoryStream.ToArray(), "image/png");
        }


        private static GraphicsPath CreateGraphicsPath(RectangleF bounds, Bitmap bitmap, SqlGeography geography)
        {
            var builder = new GraphicsPathBuilder(bounds, bitmap.Size);
            var gp = builder.Build(geography);
            return gp;
        }

        private static RectangleF CreateBBox(string bbox)
        {
            var numbers = bbox.Split(new char[] { ',' }).ToList();
            var floats = numbers.Select(number => float.Parse(number)).ToList();
            return RectangleF.FromLTRB(floats[0], floats[1], floats[2], floats[3]);
        }
    }
}
