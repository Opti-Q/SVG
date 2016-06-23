using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;

namespace Svg
{
    public class SvgElementAttributeProvider : ISvgElementAttributeProvider
    {
        private static readonly ConcurrentDictionary<Type,IEnumerable<SvgElement.PropertyAttributeTuple>> PropertyCache = new ConcurrentDictionary<Type, IEnumerable<SvgElement.PropertyAttributeTuple>>();
        private static readonly ConcurrentDictionary<Type, IEnumerable<SvgElement.EventAttributeTuple>> EventCache = new ConcurrentDictionary<Type, IEnumerable<SvgElement.EventAttributeTuple>>();

        public IEnumerable<SvgElement.PropertyAttributeTuple> GetPropertyAttributes(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            IEnumerable<SvgElement.PropertyAttributeTuple> attrs;
            if (PropertyCache.TryGetValue(instance.GetType(), out attrs))
            {
                return attrs;
            }

            attrs  = (from PropertyDescriptor a in TypeDescriptor.GetProperties(instance)
            let attribute = a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute//a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute
            where attribute != null
            select new SvgElement.PropertyAttributeTuple { Property = new SvgPropertyDescriptor(a), Attribute = attribute }).ToArray();

            var visibilityAttribute = attrs.SingleOrDefault(a => a.Attribute.Name.ToLower() == "visibility");
            if(visibilityAttribute != null)
                ((SvgPropertyDescriptor)visibilityAttribute.Property).Converter = new SvgTypeConverter(new SvgBoolConverter());

            PropertyCache.AddOrUpdate(instance.GetType(), attrs, (key, oldValue) => attrs);

            return attrs;
        }

        public IEnumerable<SvgElement.EventAttributeTuple> GetEventAttributes(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            IEnumerable<SvgElement.EventAttributeTuple> attrs;
            if (EventCache.TryGetValue(instance.GetType(), out attrs))
            {
                return attrs;
            }
            attrs = from EventDescriptor a in TypeDescriptor.GetEvents(instance)
                   let attribute = a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute
                   where attribute != null
                   select new SvgElement.EventAttributeTuple { Event = a.ComponentType.GetField(a.Name, BindingFlags.Instance | BindingFlags.NonPublic), Attribute = attribute };

            EventCache.AddOrUpdate(instance.GetType(), attrs, (key, oldValue) => attrs);

            return attrs;
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