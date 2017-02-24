using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public abstract class Color
    {
        public static Color Create(int r, int g, int b)
        {
            return Engine.Factory.CreateColorFromArgb(255, r, g, b);
        }

        public static Color Create(int a, int r, int g, int b)
        {
            return Engine.Factory.CreateColorFromArgb(a, r, g, b);
        }

        public static Color Create(string hex)
        {
            return Engine.Factory.CreateColorFromHexString(hex);
        }

        public abstract string Name { get; }
        public abstract bool IsKnownColor { get; }
        public abstract bool IsSystemColor { get; }
        public abstract bool IsNamedColor { get; }
        public abstract bool IsEmpty { get; }
        public abstract byte A { get; }
        public abstract byte R { get; }
        public abstract byte G { get; }
        public abstract byte B { get; }
        public abstract float GetBrightness();
        public abstract float GetSaturation();
        public abstract float GetHue();
        public abstract int ToArgb();
    }
}
