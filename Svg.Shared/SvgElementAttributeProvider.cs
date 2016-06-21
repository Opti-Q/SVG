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
            var attrs  = (from PropertyDescriptor a in TypeDescriptor.GetProperties(instance)
            let attribute = a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute//a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute
            where attribute != null
            select new SvgElement.PropertyAttributeTuple { Property = new SvgPropertyDescriptor(a), Attribute = attribute }).ToArray();

            var visibilityAttribute = attrs.SingleOrDefault(a => a.Attribute.Name.ToLower() == "visibility");
            if(visibilityAttribute != null)
                ((SvgPropertyDescriptor)visibilityAttribute.Property).Converter = new SvgTypeConverter(new SvgBoolConverter());

            return attrs;
        }

        public IEnumerable<SvgElement.EventAttributeTuple> GetEventAttributes(object instance)
        {
            return from EventDescriptor a in TypeDescriptor.GetEvents(instance)
                   let attribute = a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute
                   where attribute != null
                   select new SvgElement.EventAttributeTuple { Event = a.ComponentType.GetField(a.Name, BindingFlags.Instance | BindingFlags.NonPublic), Attribute = attribute };
        }
    }

    public class SvgPropertyDescriptor : IPropertyDescriptor
    {
        private readonly PropertyDescriptor _desc;
        private ITypeConverter _conv;

        public SvgPropertyDescriptor(PropertyDescriptor desc)
        {
            _desc = desc;
        }

        public ITypeConverter Converter
        {
            get { return _conv ?? (_conv = new SvgTypeConverter(_desc.Converter)); }
            set { _conv = value; }
        }

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