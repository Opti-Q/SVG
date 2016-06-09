using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface SizeF
    {
        bool IsEmpty { get; }
        float Width { get; set; }
        float Height { get; set; }
    }
}
