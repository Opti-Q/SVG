using System;
using Android.Graphics;

namespace Svg.Platform
{
    public abstract class AndroidBrushBase : IDisposable
    {
        private Paint _paint;

        public Paint Paint
        {
            get
            {
                if (_paint == null)
                {
                    _paint = CreatePaint();
                    _paint.SetStyle(Paint.Style.Fill);
                }
                return _paint;
            }
        }
        protected abstract Paint CreatePaint();

        public virtual void Dispose()
        {
            _paint?.Dispose();
            _paint = null;
        }
    }
}