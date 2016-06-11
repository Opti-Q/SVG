using Android.Graphics;

namespace Svg.Platform
{
    public class AndroidSolidBrush : SolidBrush, IAndroidShader
    {
        private readonly Color _color;

        public AndroidSolidBrush(Color color)
        {
            _color = color;
        }

        public void Dispose()
        {
            
        }

        public void ApplyTo(Paint paint)
        {
            paint.Color = _color;
        }
    }
}