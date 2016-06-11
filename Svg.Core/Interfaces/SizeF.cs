using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public abstract class SizeF
    {
        public abstract bool IsEmpty { get; }
        public abstract float Width { get; set; }
        public abstract float Height { get; set; }

        public static SizeF operator +(SizeF c1, SizeF c2)
        {
            return Engine.Factory.CreateSizeF(c1.Width + c2.Width, c1.Height + c2.Height);
        }

        public static SizeF operator -(SizeF c1, SizeF c2)
        {
            return Engine.Factory.CreateSizeF(c1.Width - c2.Width, c1.Height - c2.Height);
        }

        public static SizeF operator *(SizeF c1, SizeF c2)
        {
            return Engine.Factory.CreateSizeF(c1.Width / c2.Width, c1.Height / c2.Height);
        }

        public static SizeF operator /(SizeF c1, SizeF c2)
        {
            return Engine.Factory.CreateSizeF(c1.Width / c2.Width, c1.Height / c2.Height);
        }

        public static bool operator ==(SizeF c1, SizeF c2)
        {
            return c1?.Width == c2?.Width && c1?.Height == c2?.Height;
        }

        public static bool operator !=(SizeF c1, SizeF c2)
        {
            return c1?.Width != c2?.Width || c1?.Height != c2?.Height;
        }
    }
}
