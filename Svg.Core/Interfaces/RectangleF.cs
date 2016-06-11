using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public abstract class RectangleF
    {
        public abstract void Inflate(float x, float y);
        public abstract void Inflate(SizeF size);
        public abstract void Intersect(RectangleF rect);
        public abstract bool Contains(float x, float y);
        public abstract bool Contains(PointF pt);
        public abstract bool Contains(RectangleF rect);
        public abstract bool IntersectsWith(RectangleF rect);
        public abstract void Offset(float x, float y);
        public abstract void Offset(PointF pos);
        public abstract float Bottom { get; }
        public abstract float Height { get; set; }
        public abstract bool IsEmpty { get; }
        public abstract float Left { get; }
        public abstract PointF Location { get; set; }
        public abstract float Right { get; }
        public abstract SizeF Size { get; set; }
        public abstract float Top { get; }
        public abstract float Width { get; set; }
        public abstract float X { get; set; }
        public abstract float Y { get; set; }
        public abstract RectangleF UnionAndCopy(RectangleF other);
        public abstract RectangleF InflateAndCopy(float x, float y);



        public static bool operator ==(RectangleF c1, RectangleF c2)
        {
            return c1?.X == c2?.X && c1?.Y == c2?.Y && c1?.Width == c2?.Width && c1?.Height == c2?.Height;
        }

        public static bool operator !=(RectangleF c1, RectangleF c2)
        {
            return c1?.X != c2?.X || c1?.Y != c2?.Y || c1?.Width != c2?.Width || c1?.Height != c2?.Height;
        }
    }
}
