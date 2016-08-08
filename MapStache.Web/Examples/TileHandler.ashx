<%@ WebHandler Language="C#" Class="TileHandler" %>

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using System.Web.SessionState;


public class TileHandler : IRequiresSessionState,IHttpHandler
{
    
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "image/png";
        var x = int.Parse(context.Request.QueryString["x"]);
        var y = int.Parse(context.Request.QueryString["y"]);
        var z = int.Parse(context.Request.QueryString["z"]);
      
        var bitmap = new Bitmap(256, 256);
        var graphics = Graphics.FromImage(bitmap);
        for (int i = 128; i > 0; i = i - 10)
        {
            var brush =new SolidBrush(Color.FromArgb(i,0,0));
            graphics.FillEllipse(brush, 128 - i, 128 - i, i * 2, i * 2);

        }
        bitmap.Save(context.Response.OutputStream,ImageFormat.Png);
        //context.Response.BinaryWrite(bitmap);
    }

    
    public bool IsReusable {
        get {
            return false;
        }
    }

}