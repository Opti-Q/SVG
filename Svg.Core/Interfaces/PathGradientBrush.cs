
using System.Drawing;
using System.Drawing.Drawing2D;
using Svg.Interfaces;

namespace Svg
{
    public interface PathGradientBrush : Brush
    {
        PointF CenterPoint { get; set; }
        ColorBlend InterpolationColors { get; set; }
    }
}