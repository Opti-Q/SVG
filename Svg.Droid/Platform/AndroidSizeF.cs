using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidSizeF : SizeF
    {
        public AndroidSizeF(PointF pt) : base(pt)
        {
        }

        public AndroidSizeF(SizeF size) : base(size)
        {
        }

        public AndroidSizeF(float width, float height) : base(width, height)
        {
        }

        public static implicit operator AndroidSizeF(System.Drawing.SizeF other)
        {
            return new AndroidSizeF(other.Width, other.Height);
        }
        public static implicit operator System.Drawing.SizeF(AndroidSizeF other)
        {
            return new System.Drawing.SizeF(other.Width, other.Height);
        }

    }
}