using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using Mapstache;
using Microsoft.SqlServer.Types;
using WebStache;

namespace MapStache.App.Controllers
{
    public class XyzController : Controller
    {
        public RectangleF GetBoundingBoxInLatLng(int tileX, int tileY, int zoom)
        {
            var ymax = 1 << zoom;
            tileY = ymax - tileY - 1;
            var lonlat1 = TileSystemHelper.PixelXYToLatLong(new Point((tileX * 256), (tileY * 256)), zoom);
            var lonlat2 = TileSystemHelper.PixelXYToLatLong(new Point(((tileX + 1) * 256), ((tileY + 1) * 256)), zoom);
            return RectangleF.FromLTRB(lonlat1.X, lonlat2.Y, lonlat2.X, lonlat1.Y);
        }

        //[Route("tms1/{name}/{z}/{x}/{y}.xxx")]
        //[HttpGet]
        public ActionResult Index(int x, int y, int z)
        {
            var ymax = 1 << z;
            y = ymax - y - 1;
            var memoryStream = new MemoryStream();
            using (var bitmap = new Bitmap(256, 256))
            using (var g = Graphics.FromImage(bitmap))
            using (var pen = new Pen(Color.Blue, 2.0f))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                var boundsGeographyLL = GetBoundingBoxInLatLng(x, y, z);
                if (boundsGeographyLL.Bottom > 0)
                {
                    var states = new GeometryDataSource().Query(boundsGeographyLL.ToSqlGeography(), "sde.COUNTY_ESRI_WGS84");
                    var builder = new GraphicsPathBuilder(SphericalMercator.FromLonLat(boundsGeographyLL), new Size(256 ,256));
                    foreach (var state in states)
                    {
                        var geography = (SqlGeography) state["Shape"];
                        {
                            using (var gp = builder.Build(geography))
                            {
                                g.DrawPath(pen, gp);
                            }
                        }
                    }
                }
                bitmap.Save(memoryStream, ImageFormat.Png);
            }
            return File(memoryStream.ToArray(), "image/png");
        }


    }
}
