using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public abstract class SizeF
    {
        public abstract bool IsEmpty { get; }
        public abstract float Width { get; set; }
        public abstract float Height { get; set; }
    }
}
