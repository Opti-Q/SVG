using System;
using System.Collections.Generic;
using Svg.Interfaces;

namespace Svg
{
    public static class Engine
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, Func<object>> _serviceRegistry = new Dictionary<Type, Func<object>>();
        private static IFactory _factory = null;
        private static ISvgTypeDescriptor _typeDescriptor = null;
        private static ISvgElementAttributeProvider _attributeProvider = null;
        private static ILogger _logger = null;

        public static IFactory Factory
        {
            get
            {
                return _factory;
            }
        }

        public static ISvgTypeDescriptor TypeDescriptor
        {
            get
            {
                return _typeDescriptor;
            }
        }

        public static ISvgElementAttributeProvider SvgElementAttributeProvider
        {
            get
            {
                return _attributeProvider;
            }
        }

        public static ILogger Logger
        {
            get { return _logger; }
        }

        public static void Register<TInterface, TImplementation>(Func<TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            lock (_lock)
            {
                _serviceRegistry[typeof(TInterface)] = factory;

                // store IFactory separatly as it is used more often
                if (typeof(TInterface) == typeof(IFactory))
                    _factory = (IFactory)factory();
                if (typeof(TInterface) == typeof(ISvgTypeDescriptor))
                    _typeDescriptor = (ISvgTypeDescriptor)factory();
                if (typeof(TInterface) == typeof(ISvgElementAttributeProvider))
                    _attributeProvider = (ISvgElementAttributeProvider)factory();
                if (typeof(TInterface) == typeof(ILogger))
                    _logger = (ILogger)factory();
            }
        }

        public static TInterface Resolve<TInterface>()
            where TInterface : class
        {
            lock (_lock)
            {
                Func<object> result;
                if (_serviceRegistry.TryGetValue(typeof(TInterface), out result))
                {
                    return (TInterface)result();
                }
                throw new InvalidOperationException($"Interface {typeof(TInterface).FullName} could not be resovled. Maybe the platform has not been initialized yet?");
            }
        }
    }
}
