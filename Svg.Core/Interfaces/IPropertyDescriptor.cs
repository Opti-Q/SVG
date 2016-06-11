using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface IPropertyDescriptor
    {
        ITypeConverter Converter { get; }
        object GetValue(object instance);
    }
}
