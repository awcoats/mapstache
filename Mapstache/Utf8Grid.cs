using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;

namespace MapStache
{
    public class Utf8Grid: IDisposable
    {
        [JsonProperty("keys")]
        public List<string> Keys { get; set; }
        [JsonProperty("data")]
        private Dictionary<string, object> Data { get; set; }
        [JsonProperty("grid")]
        private List<string> Grid { get; set; }

        private Bitmap _bitmap;
        private Graphics _graphics;
        private readonly int _utfGridResolution;
        private readonly GraphicsPathBuilder _graphicsPathBuilder;

        public Utf8Grid(int utfGridResolution, int tileX, int tileY, int zoom)
        {
            _utfGridResolution = utfGridResolution;
            var size = new Size(256 / utfGridResolution, 256 / utfGridResolution);
            _bitmap = new Bitmap(size.Width, size.Height,PixelFormat.Format32bppRgb);
            _graphics = Graphics.FromImage(_bitmap);
            var bbox = GetBoundingBoxInLatLngWithMargin(tileX, tileY, zoom);
            _graphicsPathBuilder = new GraphicsPathBuilder(SphericalMercator.FromLonLat(bbox), new Size(256 / _utfGridResolution, 256 / _utfGridResolution));
            this.Keys = new List<string>();
            this.Data = new Dictionary<string, object>();
            this.Grid = new List<string>();
        }

        public RectangleF GetBoundingBoxInLatLngWithMargin(int tileX, int tileY, int zoom)
        {
            var lonlat1 = TileSystemHelper.PixelXYToLatLong(new Point((tileX * 256), (tileY * 256)), zoom);
            var lonlat2 = TileSystemHelper.PixelXYToLatLong(new Point(((tileX + 1) * 256), ((tileY + 1) * 256)), zoom);
            return RectangleF.FromLTRB(lonlat1.X, lonlat2.Y, lonlat2.X, lonlat1.Y);
        }

        public void FillPolygon(SqlGeography geography,int i, object data=null)
        {
              using (var gp = _graphicsPathBuilder.Build(geography))
              using (var brush = Utf8Grid.CreateBrush(i))
              {
                  _graphics.FillPath(brush,gp);
              }
            if (data!=null)
            {
                this.Data.Add(i.ToString(),data);
            }
        }
       
        
        public string CreateUtfGridJson()
        {
            //bitmap.Save("c:\\temp\\utg8.png");
            var bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly,
                                             _bitmap.PixelFormat);

            var ptr = bitmapData.Scan0;
            int bytes = Math.Abs(bitmapData.Stride) * _bitmap.Height;
            var rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            var uniqueValues = new List<int>();
            var grid = new int[_bitmap.Width, _bitmap.Height];

            for (int row = 0; row < _bitmap.Height; row++)
            {
                var start = row * bitmapData.Stride;
                for (int x = 0; x < _bitmap.Width; x++)
                {
                    var value = RgbToInt(start, rgbValues, x);
                    if (uniqueValues.Contains(value) == false)
                    {
                        uniqueValues.Add(value);
                    }
                    grid[x, row] = value;
                }
            }
            uniqueValues.Sort();
            _bitmap.UnlockBits(bitmapData);

         
            for (int y = 0; y < _bitmap.Height; y++)
            {
                var sb = new StringBuilder();
                for (int x = 0; x < _bitmap.Width; x++)
                {
                    var key = (grid[x, y]);
                    var id = uniqueValues.IndexOf(key);
                    id = id + 32;
                    if (id >= 34)
                    {
                        id = id + 1;
                    }
                    if (id >= 92)
                    {
                        id = id + 1;
                    }
                    sb.Append(char.ConvertFromUtf32(id));
                }
                this.Grid.Add(sb.ToString());
            }

            if (uniqueValues.Contains(0))
            {
                // remove 0 since that is taken care of by ""
                uniqueValues.Remove(0);
                this.Keys.Add("");
            }
           

            uniqueValues.ForEach(value => this.Keys.Add(value.ToString()));

            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }

        public static int RgbToInt(int start, byte[] rgbValues, int x)
        {
            var v = new byte[] {rgbValues[start], rgbValues[start + 1], rgbValues[start + 2], rgbValues[start + 3]};
            var r = rgbValues[start + (x * 4) + 2];
            var g = rgbValues[start + (x * 4) + 1];
            var b = rgbValues[start + (x * 4)];
            var value = r + (g * 256) + (b * 65536);
            return value;
        }

        public static Color IntToRgb(int p)
        {
            var r = p & 255;
            var g = (p >> 8) & 255;
            var b = (p >> 16) & 255;
            var color = Color.FromArgb(r, g, b);
            return color;
        }

        public static Brush CreateBrush(int p)
        {
            var color = IntToRgb(p);
            var brush = new SolidBrush(color);
            return brush;
        }

        public static Pen CreatePen(int p)
        {
            var color = IntToRgb(p);
            var pen = new Pen(color);
            return pen;
        }

        public Graphics CreateGraphics()
        {
            return Graphics.FromImage(_bitmap);
        }

        public void Dispose()
        {
            if (_bitmap!=null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
            if (_graphics!=null)
            {
                _graphics.Dispose();
                _graphics = null;
            }
        }
    }
}
