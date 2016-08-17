using System;
using System.Drawing;
using Microsoft.SqlServer.Types;

namespace MapStache
{
    public static class SqlGeometryExtensions
    {
        public static RectangleF ToRectangleF(this SqlGeometry geometry)
        {
            if (geometry.IsNull || geometry.STIsEmpty())
            {
                return RectangleF.Empty;
            }

            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            for (int i = 0; i < geometry.STNumPoints(); i++)
            {
                var coord = geometry.STPointN(i + 1);
                minX = Math.Min(minX, coord.STX.Value);
                maxX = Math.Max(maxX, coord.STY.Value);
                minY = Math.Min(minY, coord.STX.Value);
                maxY = Math.Max(maxY, coord.STY.Value);
            }
            return RectangleF.FromLTRB((float)minX, (float)minY, (float)maxX, (float)maxY);
        }
    }
}
