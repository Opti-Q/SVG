using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Interfaces
{
    public interface ISvgSource
    {
        Stream GetStream();
        Stream GetFileRelativeTo(Uri relativePath);
    }
}
