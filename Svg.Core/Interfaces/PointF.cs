using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface PointF
    {
        bool IsEmpty { get; }
        float X { get; set; }
        float Y { get; set; }
    }
}
