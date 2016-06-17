using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Core.Interfaces
{
    public interface ICanInvalidateCanvas
    {
        void FireInvalidateCanvas();
    }
}
