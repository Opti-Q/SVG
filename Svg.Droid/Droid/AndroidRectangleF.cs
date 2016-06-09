using Svg.Interfaces;

namespace Svg.Droid
{
    public class AndroidRectangleF : RectangleF
    {
        System.Drawing.RectangleF _inner;

        public AndroidRectangleF(float left, float top, float width, float height)
        {
            _inner = new System.Drawing.RectangleF(left, top, width, height);
        }

        public void Inflate(float x, float y)
        {
            _inner.Inflate(x, y);
        }

        public void Inflate(SizeF size)
        {
            _inner.Inflate(size.Width, size.Height);
        }

        public void Intersect(RectangleF rect)
        {
            _inner.Intersect(((AndroidRectangleF)rect)._inner);
        }

        public bool Contains(float x, float y)
        {
            return _inner.Contains(x, y);
        }

        public bool Contains(PointF pt)
        {
            return _inner.Contains(pt.X, pt.Y);
        }

        public bool Contains(RectangleF rect)
        {
            return _inner.Contains(((AndroidRectangleF) rect)._inner);
        }

        public bool IntersectsWith(RectangleF rect)
        {
            return _inner.IntersectsWith(((AndroidRectangleF) rect)._inner);
        }

        public void Offset(float x, float y)
        {
            _inner.Offset(x, y);
        }

        public void Offset(PointF pos)
        {
            _inner.Offset(pos.X, pos.Y);
        }

        public float Bottom => _inner.Bottom;
        public float Height
        {
            get { return _inner.Height; }
            set { _inner.Height = value; }
        }

        public bool IsEmpty => _inner.IsEmpty;
        public float Left => _inner.Left;
        public PointF Location
        {
            get { return (AndroidPointF)_inner.Location; }
            set { _inner.Location = (AndroidPointF)value; }
        }

        public float Right => _inner.Right;
        public SizeF Size
        {
            get { return (AndroidSizeF)_inner.Size; }
            set { _inner.Size = (AndroidSizeF)value; }
        }

        public float Top => _inner.Top;
        public float Width
        {
            get { return _inner.Width; }
            set { _inner.Width = value; }
        }

        public float X
        {
            get { return _inner.X; }
            set { _inner.X = value; }
        }

        public float Y
        {
            get { return _inner.Y; }
            set { _inner.Y = value; }
        }
    }
}