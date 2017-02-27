using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;

namespace Svg.Shared
{
    internal class SvgTypeDescriptor : ISvgTypeDescriptor
    {
        private readonly ISvgTypeConverterRegistry _registry;

        public SvgTypeDescriptor(ISvgTypeConverterRegistry registry)
        {
            _registry = registry;
        }

        public IEnumerable<Attribute> GetAttributes(object obj)
        {
            if (obj == null)
                return Enumerable.Empty<Attribute>();

            return obj.GetType().GetTypeInfo().GetCustomAttributes<Attribute>();
        }

        public IEnumerable<EventInfo> GetEvents(object obj)
        {
            if (obj == null)
                return Enumerable.Empty<EventInfo>();

            return obj.GetType().GetTypeInfo().DeclaredEvents;
        }

        public IEnumerable<PropertyInfo> GetProperties(object obj)
        {
            if (obj == null)
                return Enumerable.Empty<PropertyInfo>();

            return obj.GetType().GetTypeInfo().DeclaredProperties;
        }

        public ITypeConverter GetConverter(Type type)
        {
            return _registry.Get(type);
        }
    }
}
