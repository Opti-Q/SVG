using System;
using System.Collections.Generic;
using Svg.Interfaces;
using Svg.Shared;

namespace Svg
{
    public static class SvgEngine
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, Func<object>> _serviceRegistry = new Dictionary<Type, Func<object>>();
        private static readonly SvgElementFactory _elementFactory = new SvgElementFactory();
        private static readonly SvgTypeConverterRegistry _typeConverterRegistry = new SvgTypeConverterRegistry();
        private static readonly SvgCachingService _cachingService = new SvgCachingService();
        private static ISvgTypeDescriptor _typeDescriptor = new SvgTypeDescriptor(_typeConverterRegistry);
        private static IFactory _factory;
        private static ISvgElementAttributeProvider _attributeProvider;
        private static ILogger _logger;
        private static bool _initialized;

        public static IFactory Factory
        {
            get
            {
                EnsureInitialized();
                return _factory;
            }
        }

        public static ISvgTypeDescriptor TypeDescriptor
        {
            get
            {
                EnsureInitialized();
                return _typeDescriptor;
            }
        }

        public static ISvgElementAttributeProvider SvgElementAttributeProvider
        {
            get
            {
                EnsureInitialized();
                return _attributeProvider;
            }
        }

        public static ILogger Logger
        {
            get
            {
                EnsureInitialized(); return _logger;
            }
        }

        public static ISvgTypeConverterRegistry TypeConverterRegistry
        {
            get
            {
                EnsureInitialized();
                return _typeConverterRegistry;
            }
        }

        public static void RegisterSingleton<TInterface>(Func<TInterface> factory)
            where TInterface : class
        {
            EnsureInitialized();
            var singleton = new Singleton<TInterface>(factory);
            RegisterInternal(() => singleton.Instance);
        }
        
        public static void Register<TInterface>(Func<TInterface> factory)
            where TInterface : class
        {
            EnsureInitialized();

            RegisterInternal(factory);
        }

        private static void RegisterInternal<TInterface>(Func<TInterface> factory) where TInterface : class
        {
            lock (_lock)
            {
                _serviceRegistry[typeof(TInterface)] = factory;

                // store IFactory separatly as it is used more often
                if (typeof(TInterface) == typeof(IFactory))
                    _factory = (IFactory) factory();
                if (typeof(TInterface) == typeof(ISvgTypeDescriptor))
                    _typeDescriptor = (ISvgTypeDescriptor) factory();
                if (typeof(TInterface) == typeof(ISvgElementAttributeProvider))
                    _attributeProvider = (ISvgElementAttributeProvider) factory();
                if (typeof(TInterface) == typeof(ILogger))
                    _logger = (ILogger) factory();
            }
        }

        public static TInterface Resolve<TInterface>()
            where TInterface : class
        {
            EnsureInitialized();

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

        public static TInterface TryResolve<TInterface>()
            where TInterface : class
        {
            EnsureInitialized();

            lock (_lock)
            {
                Func<object> result;
                if (_serviceRegistry.TryGetValue(typeof(TInterface), out result))
                {
                    return (TInterface)result();
                }
                return null;
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            lock (_lock)
            {
                RegisterBaseServices();
                _initialized = true;
            }
        }

        private static void RegisterBaseServices()
        {
            RegisterInternal<ISvgElementFactory>(() => _elementFactory);
            RegisterInternal<ISvgTypeConverterRegistry>(() => _typeConverterRegistry);
            RegisterInternal<ISvgTypeDescriptor>(() => (SvgTypeDescriptor)_typeDescriptor);
            RegisterInternal<ISvgCachingService>(() => _cachingService);
        }

        private class Singleton<TInterface>
        {
            private Lazy<TInterface> _instanceHolder;
            public Singleton(Func<TInterface> factory)
            {
                _instanceHolder = new Lazy<TInterface>(factory);
            }

            public TInterface Instance => _instanceHolder.Value;
        }
    }
}
