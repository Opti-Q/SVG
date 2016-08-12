using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
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
        private static bool _initialized = false;

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
                EnsureInitialized(); return _logger; }
        }

        public static void Register<TInterface, TImplementation>(Func<TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            EnsureInitialized();

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
                var currentdomain = typeof(string).GetTypeInfo().Assembly.GetType("AppDomain").GetRuntimeProperty("CurrentDomain").GetMethod.Invoke(null, new object[] { });
                var getassemblies = currentdomain.GetType().GetRuntimeMethod("GetAssemblies", new Type[] { });
                var assemblies = getassemblies.Invoke(currentdomain, new object[] { }) as Assembly[];

                var platformSetupAttribute =
                    assemblies.SelectMany(a => a.CustomAttributes.OfType<SvgPlatformAttribute>()).FirstOrDefault();

                if (platformSetupAttribute == null)
                    throw new InvalidCastException("No platform specific SVG setup seems to exist");

                // create instance and call "init"

                _initialized = true;
            }
        }
    }
}
