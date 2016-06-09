using System;
using System.Collections.Generic;

namespace Svg
{
    public class SvgSetup
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, Func<object>> _serviceRegistry = new Dictionary<Type, Func<object>>();

        public static void Register<TInterface, TImplementation>(Func<TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            lock (_lock)
            {
                _serviceRegistry[typeof(TInterface)] = factory;
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
