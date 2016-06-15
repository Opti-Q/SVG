using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        void FillEntireCanvasWithColor(Svg.Interfaces.Color color);
        Matrix Matrix { get; }

        Graphics Graphics { get; }
    }
}
