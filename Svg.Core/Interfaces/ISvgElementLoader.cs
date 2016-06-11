using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Interfaces
{
    public interface ISvgElementLoader
    {
        SvgElement Load(Uri uri);
    }
}
