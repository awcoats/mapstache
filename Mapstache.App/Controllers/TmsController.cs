using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using Mapstache;
using Microsoft.SqlServer.Types;

namespace Utf8GridApplication.Controllers
{
    public class TmsController : Controller
    {
        //
        // GET: /Tms/
        public RectangleF GetBoundingBoxInLatLngWithMargin(int tileX, int tileY, int zoom)
        {
            var lonlat1 = TileSystemHelper.PixelXYToLatLong(new Point((tileX * 256) , (tileY * 256) ), zoom);
            var lonlat2 = TileSystemHelper.PixelXYToLatLong(new Point(((tileX + 1) * 256) , ((tileY + 1) * 256)), zoom);
            return RectangleF.FromLTRB(lonlat1.X, lonlat2.Y, lonlat2.X, lonlat1.Y);
        }

        public ActionResult Index(string version, string layer, int x, int y, int z)
        {
            //var ymax = 1 << z;
            //y = ymax - y - 1;
            var utfgridResolution = 1;
            var bbox = GetBoundingBoxInLatLngWithMargin(x, y, z);
            var geographies = new GeometryDataSource().Query(bbox.ToSqlGeography(), "states");
            using (var memoryStream = new MemoryStream())
            using (var bitmap = new Bitmap(256 / utfgridResolution, 256 / utfgridResolution))
            using (var brush = new SolidBrush(Color.FromArgb(50,0,0,255)))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                //graphics.ScaleTransform(0.5f, 0.5f);
                var builder = new GraphicsPathBuilder(SphericalMercator.FromLonLat(bbox), new Size(256 / utfgridResolution, 256 / utfgridResolution));

                foreach (var reader in geographies)
                {
                    var geography = (SqlGeography)reader["geom"];
                    using (var gp = builder.Build(geography))
                    {
                        graphics.FillPath(brush, gp);
                        graphics.DrawPath(Pens.Red, gp);
                    }
                }
                //graphics.DrawRectangle(Pens.Purple,0,0,255,255);
                //graphics.DrawString(string.Format("{0},{1},{2}",x,y,z),SystemFonts.CaptionFont,Brushes.CornflowerBlue,90,125);

                bitmap.Save(memoryStream, ImageFormat.Png);
                return File(memoryStream.ToArray(), "image/png");
            }
        }


        public ActionResult IndexInfo(string version,string layer, int x, int y, int z )
        {
            var ymax = 1 << z;
            var tiyleY2 = ymax - y - 1;
            var quadKey = TileSystemHelper.TileXYToQuadKey(x, y, z);

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

    }
}
