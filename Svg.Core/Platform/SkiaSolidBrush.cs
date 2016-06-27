using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaSolidBrush : SkiaBrushBase, SolidBrush
    {
        private readonly SKColor _color;
        private SKPaint _p;

        public SkiaSolidBrush(Color color)
        {
            _color = new SKColor(color.R, color.G, color.B, color.A);
        }
        public SkiaSolidBrush(SKPaint paint)
        {
            _p = paint;
        }

        protected override SKPaint CreatePaint()
        {
            if (_p != null)
                return _p;

            var paint = new SKPaint();
            paint.Color = _color;
            return paint;
        }
    }
}