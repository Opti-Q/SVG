using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidPointF : PointF
    {
        public AndroidPointF(float x, float y) : base(x, y)
        {
        }

        public static implicit operator AndroidPointF(System.Drawing.PointF other)
        {
            return new AndroidPointF(other.X, other.Y);
        }
        public static implicit operator System.Drawing.PointF(AndroidPointF other)
        {
            return new System.Drawing.PointF(other.X, other.Y);
        }
        public static implicit operator AndroidPointF(Android.Graphics.PointF other)
        {
            return new AndroidPointF(other.X, other.Y);
        }
        public static implicit operator Android.Graphics.PointF(AndroidPointF other)
        {
            return new Android.Graphics.PointF(other.X, other.Y);
        }
    }
}