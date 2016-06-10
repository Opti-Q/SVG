using Svg.Interfaces;

namespace Svg.Droid
{
    public class AndroidSizeF : SizeF
    {
        private System.Drawing.SizeF _inner;

        public AndroidSizeF()
        {
            _inner = new System.Drawing.SizeF(0, 0);
        }
        public AndroidSizeF(float width, float height)
        {
            _inner = new System.Drawing.SizeF(width, height);
        }

        public AndroidSizeF(System.Drawing.SizeF inner)
        {
            _inner = inner;
        }

        public override bool IsEmpty => _inner.IsEmpty;

        public override float Width
        {
            get { return _inner.Width; }
            set { _inner.Width = value; }
        }
        public override float Height
        {
            get { return _inner.Height; }
            set { _inner.Height = value; }
        }

        public static implicit operator AndroidSizeF(System.Drawing.SizeF other)
        {
            return new AndroidSizeF(other);
        }

        public static implicit operator System.Drawing.SizeF(AndroidSizeF other)
        {
            return other._inner;
        }
    }
}