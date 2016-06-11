using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Svg.Interfaces;

namespace Svg
{
    public class SvgTypeDescriptor : ISvgTypeDescriptor
    {
        public IEnumerable<Attribute> GetAttributes(object obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EventInfo> GetEvents(object obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PropertyInfo> GetProperties(object obj)
        {
            throw new NotImplementedException();
        }

        public ITypeConverter GetConverter(Type type)
        {
            throw new NotImplementedException();
        }
    }
}