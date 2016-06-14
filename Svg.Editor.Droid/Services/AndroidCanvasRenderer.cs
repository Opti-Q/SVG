
using Android.Graphics;
using Svg.Core.Interfaces;
using Svg.Platform;

namespace Svg.Droid.Editor.Services
{
    public class AndroidCanvasRenderer : IRenderer
    {
        private readonly Canvas _canvas;

        public AndroidCanvasRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public int Width => _canvas.Width;

        public int Height => _canvas.Height;

        public void DrawLine(float startX, float startY, float stopX, float stopY, Pen paint)
        {
            _canvas.DrawLine(startX, startY, stopX, stopY, ((AndroidPen) paint).Paint);
        }

        public void Scale(float zoomFactor, float p1, float focusX, float focusY)
        {
            _canvas.Scale(zoomFactor, zoomFactor, focusX, focusY);
        }
    }
}