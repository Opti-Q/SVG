using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface Color
    {
        string Name { get; }
        bool IsKnownColor { get; }
        bool IsSystemColor { get; }
        bool IsNamedColor { get; }
        bool IsEmpty { get; }
        byte A { get; }
        byte R { get; }
        byte G { get; }
        byte B { get; }
        float GetBrightness();
        float GetSaturation();
        float GetHue();
        int ToArgb();
    }
}
