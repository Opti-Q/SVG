using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface ITypeConverter
    {
        object ConvertFrom(string value);
        string ConvertToString(object obj);
        bool CanConvertTo(Type type);
        object ConvertTo(object propertyValue, Type type);
    }
}
