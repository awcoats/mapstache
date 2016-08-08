using System.Drawing;
using System.Web.Mvc;
using Mapstache;
using Microsoft.SqlServer.Types;
using Utf8GridApplication.Examples;

namespace MapStache.App.Controllers
{
    public class Utf8GridController : Controller
    {
        public RectangleF GetBoundingBoxInLatLngWithMargin(int tileX, int tileY, int zoom)
        {
            var lonlat1 = TileSystemHelper.PixelXYToLatLong(new Point((tileX * 256), (tileY * 256)), zoom);
            var lonlat2 = TileSystemHelper.PixelXYToLatLong(new Point(((tileX + 1) * 256), ((tileY + 1) * 256)), zoom);
            return RectangleF.FromLTRB(lonlat1.X, lonlat2.Y, lonlat2.X, lonlat1.Y);
        }

        public ActionResult States(int x,int y, int z)
        {
            var key = string.Format(@"states\{0}\{1}\{1}", x, y, z);
            //var cachedJson = this.HttpContext.Cache[key] as string;
            //if (cachedJson != null)
            //{
            //    return new ContentResult() {Content = cachedJson, ContentType = "application/json"};
            //}

            const int utfgridResolution = 2;

            using (var utf8Grid = new Utf8Grid(utfgridResolution, x, y, z))
            {
                var bbox = GetBoundingBoxInLatLngWithMargin(x, y, z);
                if (bbox.Bottom > 0)
                {
                    var states = new StatesRepository().Query(bbox.ToSqlGeography());
                    int i = 1;
                    foreach (var state in states)
                    {
                        var geography = (SqlGeography) state["Geom"];
                        var projected = ((SqlGeography) state["Geom"]).FromLonLat().MakeValid();
                        var wkt = projected.STAsText().ToSqlString().Value;
                        utf8Grid.FillPolygon(geography, i,
                                             new {NAME = state["NAME"], POP2005 = state["ALAND"], Wkt = wkt});
                        i = i + 1;
                    }
                }
                var cachedJson = utf8Grid.CreateUtfGridJson();
                //this.HttpContext.Cache.Insert(key, cachedJson);
                return new ContentResult() { Content = cachedJson, ContentType = "application/json" };
            }
        }
    }
}
