using System;
using SkiaSharp;

namespace Svg.Platform
{
    public abstract class SkiaBrushBase : IDisposable
    {
        private SKPaint _paint;

        public SKPaint Paint
        {
            get
            {
                if (_paint == null)
                {
                    _paint = CreatePaint();
                    _paint.IsStroke = false;
                }
                return _paint;
            }
        }
        protected abstract SKPaint CreatePaint();

        protected void Reset()
        {
            _paint?.Dispose();
            _paint = null;
        }

        public virtual void Dispose()
        {
            Reset();
        }
    }
}
