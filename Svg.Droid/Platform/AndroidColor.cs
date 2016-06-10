using Svg.Droid;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidColor : Color
    {
        private System.Drawing.Color _inner;

        public AndroidColor()
        {
            _inner = new System.Drawing.Color();
        }

        public AndroidColor(byte r, byte g, byte b)
        {
            _inner = System.Drawing.Color.FromArgb(255, r, g, b);
        }

        public AndroidColor(byte a, byte r, byte g, byte b)
        {
            _inner = System.Drawing.Color.FromArgb(a, r, g, b);
        }


        public AndroidColor(byte a, Color baseColor)
        {
            _inner = System.Drawing.Color.FromArgb(a, ((AndroidColor)baseColor)._inner);
        }

        public AndroidColor(System.Drawing.Color inner)
        {
            _inner = inner;
        }

        public string Name => _inner.Name;
        public bool IsKnownColor => _inner.IsKnownColor;
        public bool IsSystemColor => _inner.IsSystemColor;
        public bool IsNamedColor => _inner.IsNamedColor;
        public bool IsEmpty => _inner.IsEmpty;
        public byte A => _inner.A;
        public byte R => _inner.R;
        public byte G => _inner.G;
        public byte B => _inner.B;
        public float GetBrightness()
        {
            return _inner.GetBrightness();
        }

        public float GetSaturation()
        {
            return _inner.GetSaturation();
        }

        public float GetHue()
        {
            return _inner.GetHue();
        }

        public int ToArgb()
        {
            return _inner.ToArgb();
        }



        public static implicit operator AndroidColor(System.Drawing.Color other)
        {
            return new AndroidColor(other);
        }

        public static implicit operator System.Drawing.Color(AndroidColor other)
        {
            return other._inner;
        }

        public static implicit operator AndroidColor(Android.Graphics.Color other)
        {
            return new AndroidColor(other.A, other.R, other.G, other.B);
        }

        public static implicit operator Android.Graphics.Color(AndroidColor other)
        {
            return other._inner.ToColor();
        }
    }
}