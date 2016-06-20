using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public abstract class PointF
    {
        public abstract bool IsEmpty { get; }
        public abstract float X { get; set; }
        public abstract float Y { get; set; }

        public static PointF operator +(PointF c1, PointF c2)
        {
            return Engine.Factory.CreatePointF(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static PointF operator -(PointF c1, PointF c2)
        {
            return Engine.Factory.CreatePointF(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static PointF operator *(PointF c1, PointF c2)
        {
            return Engine.Factory.CreatePointF(c1.X / c2.X, c1.Y / c2.Y);
        }

        public static PointF operator /(PointF c1, PointF c2)
        {
            return Engine.Factory.CreatePointF(c1.X / c2.X, c1.Y / c2.Y);
        }

        public static bool operator ==(PointF c1, PointF c2)
        {
            return c1?.X == c2?.X && c1?.Y == c2?.Y;
        }

        public static bool operator !=(PointF c1, PointF c2)
        {
            return c1?.X != c2?.X || c1?.Y != c2?.Y;
        }

        public override string ToString()
        {
            return $"x:{X.ToString("00.0")} y:{Y.ToString("00.0")}";
        }
    }
}
