using Android.Graphics;
using Svg.Core.Interfaces;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg.Droid.Editor.Services
{
    public class AndroidCanvasRenderer : IRenderer
    {
        private readonly Canvas _canvas;

        public AndroidCanvasRenderer(Canvas canvas)
        {
            _canvas = canvas;
            Graphics = new AndroidGraphics(canvas);
        }

        public int Width => _canvas.Width;

        public int Height => _canvas.Height;

        public void DrawLine(float startX, float startY, float stopX, float stopY, Pen pen)
        {
            _canvas.DrawLine(startX, startY, stopX, stopY, ((AndroidPen) pen).Paint);
        }

        public void Scale(float zoomFactor, float focusX, float focusY)
        {
            _canvas.Scale(zoomFactor, zoomFactor, focusX, focusY);
        }

        public void Translate(float deltaX, float deltaY)
        {
            _canvas.Translate(deltaX, deltaY);
        }

        public void DrawCircle(float x, float y, int radius, Pen pen)
        {
            _canvas.DrawCircle(x, y, radius, ((AndroidPen)pen).Paint);
        }

        public void DrawRectangle(RectangleF rectangleF, Pen pen)
        {
            _canvas.DrawRect((RectF)(AndroidRectangleF) rectangleF, ((AndroidPen) pen).Paint);
        }

        public void DrawPath(GraphicsPath path, Pen pen)
        {
            _canvas.DrawPath(((AndroidGraphicsPath)path).Path, ((AndroidPen)pen).Paint);
        }

        public void FillEntireCanvasWithColor(Svg.Interfaces.Color color)
        {
            var c = (AndroidColor) color;
            _canvas.DrawColor(c);
        }

        public Graphics Graphics { get; }
    }
}