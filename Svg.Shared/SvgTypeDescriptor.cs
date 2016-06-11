using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Svg.Interfaces;

namespace Svg
{
    public class SvgTypeDescriptor : ISvgTypeDescriptor
    {
        public IEnumerable<Attribute> GetAttributes(object obj)
        {
            return TypeDescriptor.GetAttributes(obj).Cast<Attribute>();
        }

        public IEnumerable<EventInfo> GetEvents(object obj)
        {
            return TypeDescriptor.GetEvents(obj).Cast<EventInfo>();
        }

        public IEnumerable<PropertyInfo> GetProperties(object obj)
        {
            return TypeDescriptor.GetProperties(obj).Cast<PropertyInfo>();
        }

        public ITypeConverter GetConverter(Type type)
        {
            return new SvgTypeConverter(TypeDescriptor.GetConverter(type));
        }
    }
}