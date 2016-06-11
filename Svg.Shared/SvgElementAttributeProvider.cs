using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;

namespace Svg
{
    public class SvgElementAttributeProvider : ISvgElementAttributeProvider
    {
        public IEnumerable<SvgElement.PropertyAttributeTuple> GetPropertyAttributes(object instance)
        {
            return from PropertyDescriptor a in TypeDescriptor.GetProperties(this)
            let attribute = a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute//a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute
            where attribute != null
            select new SvgElement.PropertyAttributeTuple { Property = new SvgPropertyDescriptor(a), Attribute = attribute };
        }

        public IEnumerable<SvgElement.EventAttributeTuple> GetEventAttributes(object instance)
        {
            return from EventDescriptor a in TypeDescriptor.GetEvents(this)
                   let attribute = a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute
                   where attribute != null
                   select new SvgElement.EventAttributeTuple { Event = a.ComponentType.GetField(a.Name, BindingFlags.Instance | BindingFlags.NonPublic), Attribute = attribute };
        }
    }

    public class SvgPropertyDescriptor : IPropertyDescriptor
    {
        private readonly PropertyDescriptor _desc;

        public SvgPropertyDescriptor(PropertyDescriptor desc)
        {
            _desc = desc;
        }

        public ITypeConverter Converter => new SvgTypeConverter(_desc.Converter);
        public object GetValue(object instance)
        {
            return _desc.GetValue(instance);
        }
    }

    public class SvgTypeConverter : ITypeConverter
    {
        private readonly TypeConverter _cov;

        public SvgTypeConverter(TypeConverter cov)
        {
            _cov = cov;
        }

        public object ConvertFrom(string value)
        {
            return _cov.ConvertFrom(value);
        }

        public string ConvertToString(object obj)
        {
            return _cov.ConvertToString(obj);
        }

        public object ConvertTo(object propertyValue, Type type)
        {
            return _cov.ConvertTo(propertyValue, type);
        }

        public bool CanConvertTo(Type type)
        {
            return _cov.CanConvertTo(type);
        }
    }
}