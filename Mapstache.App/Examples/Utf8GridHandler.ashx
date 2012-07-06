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
            var x = int.Parse(context.Request.QueryString["x"]);
            var y = int.Parse(context.Request.QueryString["y"]);
            var z = int.Parse(context.Request.QueryString["z"]);

            context.Response.ContentType = "application/json";
            var utfgridResolution = 2;

            using (var utf8Grid = new Utf8Grid(utfgridResolution,x,y,z))
            {
                var bbox = GetBoundingBoxInLatLngWithMargin(x, y, z);
                var states = new StatesRepository().Query(bbox.ToSqlGeography());
                int i = 1;
                foreach (var state in states)
                {
                    var geography = (SqlGeography)state["geom"];
                    utf8Grid.FillPolygon(geography, i);
                    var geometry = ((SqlGeography)state["geom"]).FromLonLat().MakeValid();
                    var wkt = geometry.STAsText().ToSqlString().Value;
                    utf8Grid.Data.Add(i.ToString(),
                                      new { NAME = state["STATE_NAME"], POP2005 = state["POP2000"], Wkt = wkt });
                    i = i + 1;
                }
                context.Response.Write(utf8Grid.CreateUtfGridJson());
            }
        }
        /*
         * foreach (var state in states)
                {
                    var geography = (SqlGeography)state["geom"];
                    using (var gp = builder.Build(geography))
                    using (var brush = Utf8Grid.CreateBrush(i))
                    {
                        graphics.FillPath(brush, gp);
                        var geometry = ((SqlGeography)state["geom"]).FromLonLat().MakeValid();
                        var wkt = geometry.STAsText().ToSqlString().Value;
                        utf8Grid.Data.Add(i.ToString(),
                                          new { NAME = state["STATE_NAME"], POP2005 = state["POP2000"], Wkt = wkt });
                    }
                    i = i + 1;
                }
         * */
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}