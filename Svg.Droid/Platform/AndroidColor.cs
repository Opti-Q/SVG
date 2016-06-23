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

        public override string Name => _inner.Name;
        public override bool IsKnownColor => _inner.IsKnownColor;
        public override bool IsSystemColor => _inner.IsSystemColor;
        public override bool IsNamedColor => _inner.IsNamedColor;
        public override bool IsEmpty => _inner.IsEmpty;
        public override byte A => _inner.A;
        public override byte R => _inner.R;
        public override byte G => _inner.G;
        public override byte B => _inner.B;
        public override float GetBrightness()
        {
            return _inner.GetBrightness();
        }
        public override float GetSaturation()
        {
            return _inner.GetSaturation();
        }
        public override float GetHue()
        {
            return _inner.GetHue();
        }
        public override int ToArgb()
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

        public static implicit operator Android.Graphics.Color(AndroidColor color)
        {
            return new Android.Graphics.Color(color.R, color.G, color.B, color.A);
        }
    }
}