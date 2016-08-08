using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using Mapstache;
using Microsoft.SqlServer.Types;

namespace Utf8GridApplication.Controllers
{
    public class TmsController : Controller
    {

        public RectangleF GetBoundingBoxInLatLng(int tileX, int tileY, int zoom)
        {
            var ymax = 1 << zoom;
            tileY = ymax - tileY - 1;
            var lonlat1 = TileSystemHelper.PixelXYToLatLong(new Point((tileX * 256), (tileY * 256)), zoom);
            var lonlat2 = TileSystemHelper.PixelXYToLatLong(new Point(((tileX + 1) * 256), ((tileY + 1) * 256)), zoom);
            return RectangleF.FromLTRB(lonlat1.X, lonlat2.Y, lonlat2.X, lonlat1.Y);
        }

        public ActionResult IndexInfo(string version,string layer, int x, int y, int z )
        {
            var lonlat = TileSystemHelper.PixelXYToLatLong(new Point(x * 256, y * 256), z);

            var memoryStream = new MemoryStream();
            using (var bitmap = new Bitmap(256, 256))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var fontBrush = new SolidBrush(Color.Blue))
            using (var font = new Font(FontFamily.GenericMonospace, 10))
            {
               graphics.DrawLine(Pens.Red,0,0,256,256);

               graphics.DrawString(string.Format("TMS {0},{1} Zoom:{2}", x, y, z), font, fontBrush,
                                  new PointF(0, 0));
               graphics.DrawString(string.Format("Lon {0}:{1}", lonlat.X, lonlat.Y), font, fontBrush,
                                   new PointF(0, 15));
                bitmap.Save(memoryStream, ImageFormat.Png);
            }
            return File(memoryStream.ToArray(), "image/png");
        }

        public ActionResult Index(string version,string layer,int x, int y, int z)
        {
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
                    var states = new GeometryDataSource().Query(boundsGeographyLL.ToSqlGeography(), "states");
                    var builder = new GraphicsPathBuilder(SphericalMercator.FromLonLat(boundsGeographyLL), new Size(256, 256));
                    foreach (var state in states)
                    {
                        var geography = (SqlGeography)state["geom"];
                        {
                            using (var gp = builder.Build(geography))
                            {
                                g.DrawPath(pen, gp);
                            }
                        }
                    }
                }
                g.DrawRectangle(Pens.Red,0,0,255,255);
                bitmap.Save(memoryStream, ImageFormat.Png);
            }
            return File(memoryStream.ToArray(), "image/png");
        }

    
    }
}
