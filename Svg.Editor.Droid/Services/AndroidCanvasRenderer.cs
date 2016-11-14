using Android.Graphics;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Svg.Platform;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Editor.Droid.Services
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

        public void DrawPolygon(PointF[] points, Pen pen)
        {
            for (int i = 1; i < points.Length; i++)
            {
                DrawLine(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, pen);
            }
            DrawLine(points[points.Length - 1].X, points[points.Length - 1].Y, points[0].X, points[0].Y, pen);
        }

        public Graphics Graphics { get; }
    }
}