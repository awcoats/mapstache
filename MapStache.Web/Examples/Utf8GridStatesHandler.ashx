<%@ WebHandler Language="C#" Class="Utf8GridApplication.Examples.Utf8GridHandler" %>

using System.Drawing;
using System.Web;
using System.Web.SessionState;
using Mapstache;
using Microsoft.SqlServer.Types;
namespace Utf8GridApplication.Examples
{
    public class Utf8GridHandler : IRequiresSessionState, IHttpHandler
    {
        public RectangleF GetBoundingBoxInLatLngWithMargin(int tileX, int tileY, int zoom)
        {
            var lonlat1 = TileSystemHelper.PixelXYToLatLong(new Point((tileX * 256), (tileY * 256)), zoom);
            var lonlat2 = TileSystemHelper.PixelXYToLatLong(new Point(((tileX + 1) * 256), ((tileY + 1) * 256)), zoom);
            return RectangleF.FromLTRB(lonlat1.X, lonlat2.Y, lonlat2.X, lonlat1.Y);
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            
            var x = int.Parse(context.Request.QueryString["x"]);
            var y = int.Parse(context.Request.QueryString["y"]);
            var z = int.Parse(context.Request.QueryString["z"]);

            var key = string.Format(@"states\{0}\{1}\{1}", x, y, z);

            var cachedJson = context.Cache[key];
            if (cachedJson!=null)
            {
                context.Response.Write(cachedJson);
                return;
            }
           
           
            var utfgridResolution = 2;

            using (var utf8Grid = new Utf8Grid(utfgridResolution,x,y,z))
            {
                var bbox = GetBoundingBoxInLatLngWithMargin(x, y, z);
                var states = new StatesRepository().Query(bbox.ToSqlGeography());
                int i = 1;
                foreach (var state in states)
                {
                    var geography = (SqlGeography)state["Shape"];
                    var geometry = ((SqlGeography)state["Shape"]).FromLonLat().MakeValid();
                    var wkt = geometry.STAsText().ToSqlString().Value;
                    utf8Grid.FillPolygon(geography, i,new { NAME = state["NAME"], POP2005 = state["ALAND"], Wkt = wkt });
                    i = i + 1;
                }
                cachedJson = utf8Grid.CreateUtfGridJson();
                context.Cache.Insert(key,cachedJson);
                context.Response.Write(cachedJson);
            }
        }
       
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}