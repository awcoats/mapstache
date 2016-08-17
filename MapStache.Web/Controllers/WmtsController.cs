using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;

namespace MapStache.Web.Controllers
{
    public class WmtsController : Controller
    {
        //
        // GET: /Wmts/

        
        //wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=layer_id&STYLE=default&TILEMATRIXSET=matrix_id&TILEMATRIX=3&TILEROW=2&TILECOL=0&FORMAT=image%2Fjpeg
        public ActionResult Index(string service, string request, string version, string layer, string style, string matrixset,int tileMatrix, int tileRow,int tileCol, string format)
        {
            var ymax = 1 << tileMatrix;
            var tiyleY2 = ymax - tileMatrix - 1;
            var quadKey = TileSystemHelper.TileXYToQuadKey(tileRow, tileCol, tileMatrix);

            var lonlat = TileSystemHelper.PixelXYToLatLong(new Point(tileRow * 256, tileCol * 256), tileMatrix);

            var memoryStream = new MemoryStream();
            using (var bitmap = new Bitmap(256, 256))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var fontBrush = new SolidBrush(Color.Blue))
            using (var font = new Font(FontFamily.GenericMonospace, 10))
            {
                graphics.DrawLine(Pens.Red, 0, 0, 256, 256);

                graphics.DrawString(string.Format("TMS {0},{1} Zoom:{2}", tileRow, tileCol, tileMatrix), font, fontBrush,
                                   new PointF(0, 0));
                graphics.DrawString(string.Format("Lon {0}:{1}", lonlat.X, lonlat.Y), font, fontBrush,
                                    new PointF(0, 15));
                bitmap.Save(memoryStream, ImageFormat.Png);
            }
            return File(memoryStream.ToArray(), "image/png");
        }
    }
}
