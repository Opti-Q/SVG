using System;
using Android.Graphics;

namespace Svg.Platform
{
    public class AndroidSolidBrush : AndroidBrushBase, SolidBrush
    {
        private readonly Color _color;

        public AndroidSolidBrush(Color color)
        {
            _color = color;
        }

        
        protected override Paint CreatePaint()
        {
            var paint = new Paint();
            paint.Color = _color;
            return paint;
        }
    }
}