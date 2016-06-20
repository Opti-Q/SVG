using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core
{
    public interface ISvgSourceFactory
    {
        ISvgSource Create(string path);
    }
}
