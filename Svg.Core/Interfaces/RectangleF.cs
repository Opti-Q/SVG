using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface RectangleF
    {
        void Inflate(float x, float y);
        void Inflate(SizeF size);
        void Intersect(RectangleF rect);
        bool Contains(float x, float y);
        bool Contains(PointF pt);
        bool Contains(RectangleF rect);
        bool IntersectsWith(RectangleF rect);
        void Offset(float x, float y);
        void Offset(PointF pos);
        float Bottom { get; }
        float Height { get; set; }
        bool IsEmpty { get; }
        float Left { get; }
        PointF Location { get; set; }
        float Right { get; }
        SizeF Size { get; set; }
        float Top { get; }
        float Width { get; set; }
        float X { get; set; }
        float Y { get; set; }
    }
}
