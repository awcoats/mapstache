using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.SqlServer.Types;
using Mapstache;

namespace Utf8GridApplication.Controllers
{
    public class WmsController : Controller
    {
        //
        // GET: /Wms/

        public ActionResult Index(int width, int height, string bbox, string layers)
        {
            var bounds = CreateBBox(bbox);
            var boundsLL = SphericalMercator.ToLonLat(bounds);
            var boundsGeographyLL = boundsLL.ToSqlGeography();

            IEnumerable<SqlDataReader> geographies = null;
            if (layers == "states" || layers == "lakes" || layers == "zips" || layers == "hail" || layers == "tornado")
            {
                var layer = layers;
                
                geographies = new GeometryDataSource().Query(boundsGeographyLL, layer);
            }

            var fillColor = Color.FromArgb(80, 100, 100, 100);
            if (layers == "lakes")
            {
                fillColor = Color.Blue;
            }
            var memoryStream = new MemoryStream();
            using (var bitmap = new Bitmap(width, height))
            using (var brush = new SolidBrush(fillColor))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                int i = 0;
                foreach (var reader in geographies)
                {
                    var geography = (SqlGeography)reader["geom"];

                    //try
                    {
                        using (var gp = CreateGraphicsPath(bounds, bitmap, geography))
                        {
                            //g.FillPath(brush, gp);
                            g.DrawPath(Pens.Black, gp);
                            i++;
                        }
                    }
                    //catch
                    //{

                    //}
                }
                //g.DrawRectangle(Pens.Green,0,0,255,255);
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
