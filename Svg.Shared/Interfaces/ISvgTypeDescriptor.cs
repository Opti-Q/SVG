using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Svg.Interfaces
{
    public interface ISvgTypeDescriptor
    {
        IEnumerable<Attribute> GetAttributes(object obj);
        IEnumerable<EventInfo> GetEvents(object obj);
        IEnumerable<PropertyInfo> GetProperties(object obj);
        ITypeConverter GetConverter(Type type);
    }
}
