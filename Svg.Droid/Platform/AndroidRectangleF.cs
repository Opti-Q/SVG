using Android.Graphics;
using Svg.Interfaces;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Platform
{
    public class AndroidRectangleF : RectangleF
    {
        System.Drawing.RectangleF _inner;

        public AndroidRectangleF()
        {
            _inner = new System.Drawing.RectangleF();
        }
        public AndroidRectangleF(float left, float top, float width, float height)
        {
            _inner = new System.Drawing.RectangleF(left, top, width, height);
        }
        public AndroidRectangleF(System.Drawing.RectangleF inner)
        {
            _inner = inner;
        }

        public override void Inflate(float x, float y)
        {
            _inner.Inflate(x, y);
        }
        public override void Inflate(SizeF size)
        {
            _inner.Inflate(size.Width, size.Height);
        }
        public override void Intersect(RectangleF rect)
        {
            _inner.Intersect(((AndroidRectangleF)rect)._inner);
        }
        public override bool Contains(float x, float y)
        {
            return _inner.Contains(x, y);
        }
        public override bool Contains(PointF pt)
        {
            return _inner.Contains(pt.X, pt.Y);
        }
        public override bool Contains(RectangleF rect)
        {
            return _inner.Contains(((AndroidRectangleF) rect)._inner);
        }
        public override bool IntersectsWith(RectangleF rect)
        {
            return _inner.IntersectsWith(((AndroidRectangleF) rect)._inner);
        }
        public override void Offset(float x, float y)
        {
            _inner.Offset(x, y);
        }
        public override void Offset(PointF pos)
        {
            _inner.Offset(pos.X, pos.Y);
        }
        public override float Bottom => _inner.Bottom;
        public override float Height
        {
            get { return _inner.Height; }
            set { _inner.Height = value; }
        }
        public override bool IsEmpty => _inner.IsEmpty;
        public override float Left => _inner.Left;
        public override PointF Location
        {
            get { return (AndroidPointF)_inner.Location; }
            set { _inner.Location = (AndroidPointF)value; }
        }
        public override float Right => _inner.Right;
        public override SizeF Size
        {
            get { return (AndroidSizeF)_inner.Size; }
            set { _inner.Size = (AndroidSizeF)value; }
        }
        public override float Top => _inner.Top;
        public override float Width
        {
            get { return _inner.Width; }
            set { _inner.Width = value; }
        }
        public override float X
        {
            get { return _inner.X; }
            set { _inner.X = value; }
        }
        public override float Y
        {
            get { return _inner.Y; }
            set { _inner.Y = value; }
        }
        public override RectangleF UnionAndCopy(RectangleF other)
        {
            return new AndroidRectangleF(System.Drawing.RectangleF.Union(_inner, ((AndroidRectangleF)other)._inner));
        }
        public override RectangleF InflateAndCopy(float x, float y)
        {
            return new AndroidRectangleF(System.Drawing.RectangleF.Inflate(_inner, x, y));
        }
        public static implicit operator Rect(AndroidRectangleF other)
        {
            return new Rect((int)other.Left, (int)other.Top, (int)other.Width, (int)other.Height);
        }
        public static implicit operator Android.Graphics.RectF(AndroidRectangleF other)
        {
            return new Android.Graphics.RectF(other.Left, other.Top, other.Width, other.Height);
        }
    }
}