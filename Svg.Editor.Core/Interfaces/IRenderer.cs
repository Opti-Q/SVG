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
        void DrawLine(float startX, float startY, float stopX, float stopY, Pen paint);
    }
}
