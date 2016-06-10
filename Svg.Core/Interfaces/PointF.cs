using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public abstract class PointF
    {
        public abstract bool IsEmpty { get; }
        public abstract float X { get; set; }
        public abstract float Y { get; set; }
    }
}
