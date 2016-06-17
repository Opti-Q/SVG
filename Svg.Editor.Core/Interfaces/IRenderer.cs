
using Svg.Interfaces;

namespace Svg.Core.Interfaces
{
    public interface IRenderer
    {
        int Width { get; }
        int Height { get; }
        void DrawLine(float startX, float startY, float stopX, float stopY, Pen pen);
        void Scale(float zoomFactor, float focusX, float focusY);
        void Translate(float deltaX, float deltaY);
        void DrawCircle(float x, float y, int radius, Pen pen);
        void DrawRectangle(RectangleF rectangleF, Pen pen);
        void DrawPath(GraphicsPath path, Pen pen);
        void FillEntireCanvasWithColor(Svg.Interfaces.Color color);
        Matrix Matrix { get; set; }

        Graphics Graphics { get; }
    }
}
