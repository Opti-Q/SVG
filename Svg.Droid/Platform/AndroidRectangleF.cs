using Android.Graphics;
using Svg.Interfaces;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Platform
{
    public class AndroidRectangleF : RectangleF
    {
        public AndroidRectangleF(PointF location, SizeF size) : base(location, size)
        {
        }

        public AndroidRectangleF(float x, float y, float width, float height) : base(x, y, width, height)
        {
        }

        public static implicit operator Rect(AndroidRectangleF rect)
        {
            return new Rect((int)rect.X, (int)rect.Y, (int)rect.X + (int)rect.Width, (int)rect.Y + (int)rect.Height);
        }
        public static implicit operator Android.Graphics.RectF(AndroidRectangleF rect)
        {
            return new Android.Graphics.RectF(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }

        public override string ToString()
        {
            return $"x:{X} y:{Y} width:{Width} height:{Height}";
        }
    }
}