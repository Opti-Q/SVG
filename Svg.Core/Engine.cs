using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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
        private static bool _initializing = false;

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
                var assemblies = GetAppDomainAssemblies();

                // run setup
                ResolveAndRunPlatformSetup(assemblies);
                _initialized = true;

                // register platform-specific services
                RegisterPlatformSpecificServices(assemblies);
            }
        }

        private static void ResolveAndRunPlatformSetup(Assembly[] assemblies)
        {
            if (_initializing)
                return;

            try
            {
                _initializing = true;

                var platformSetupAttribute = GetAssemblyAttribute<SvgPlatformAttribute>(assemblies).SingleOrDefault();
                if (platformSetupAttribute == null)
                    throw new InvalidOperationException(
                        "No platform specific SVG setup was found. Create a setup and apply the assembly:[SvgPlatformAttribute] referencing it so the SVG.Engine can find it.");

                // create instance and call "init"
                var ctor = platformSetupAttribute.PlatformSetup.GetTypeInfo()
                    .DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
                if (ctor == null)
                    throw new InvalidOperationException(
                        $"Found platformsetup type '{platformSetupAttribute.PlatformSetup.FullName}' has no parameterless constructors!");

                // create setup
                var setup = ctor.Invoke(null);

                // call init
                var init = platformSetupAttribute.PlatformSetup.GetTypeInfo().GetDeclaredMethod("Initialize");
                if (init == null || init.GetParameters().Length != 0 || init.ReturnType != typeof(void))
                    throw new InvalidOperationException(
                        $"Could not find method 'public void Initialize()' on setup type $'{platformSetupAttribute.PlatformSetup.FullName}'");

                init.Invoke(setup, null);
            }
            finally
            {
                _initializing = false;
            }
        }

        private static void RegisterPlatformSpecificServices(Assembly[] assemblies)
        {
            foreach (var attribute in GetAssemblyAttribute<SvgServiceAttribute>(assemblies))
            {
                var ctor = attribute.Type.GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 0);

                _serviceRegistry[attribute.InterfaceType] = () => ctor.Invoke(null);
            }
        }

        private static IEnumerable<T> GetAssemblyAttribute<T>(Assembly[] assemblies)
        {
            var platformSetupAttributeTypes =
                assemblies.SelectMany(a => a.CustomAttributes.Where(ca => ca.AttributeType == typeof(T)))
                    .ToList();
            
            foreach (var pt in platformSetupAttributeTypes)
            {
                var ctor = pt.AttributeType.GetTypeInfo().DeclaredConstructors.First();
                var parameters = pt.ConstructorArguments?.Select(carg => carg.Value).ToArray();
                yield return (T) ctor.Invoke(parameters);
            }
        }
        
        private static Assembly[] GetAppDomainAssemblies()
        {
            var ass = typeof(string).GetTypeInfo().Assembly;
            var ty = ass.GetType("System.AppDomain");
            var gm = ty.GetRuntimeProperty("CurrentDomain").GetMethod;
            var currentdomain = gm.Invoke(null, new object[] {});
            var getassemblies = currentdomain.GetType().GetRuntimeMethod("GetAssemblies", new Type[] {});
            var assemblies = getassemblies.Invoke(currentdomain, new object[] {}) as Assembly[];
            return assemblies;
        }
    }
}
