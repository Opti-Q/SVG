using System;
using System.Collections.Generic;

namespace Svg
{
    public interface IServiceLocator
    {
        void Register<TInterface, TType>() where TType : TInterface;
        void Register<TInterface>(Func<TInterface> factory);
        void RegisterSingleton<TInterface>(Func<TInterface> factory);
        void RegisterSingleton<TInterface>(TInterface instance);

        T Resolve<T>();
        T TryResolve<T>();
        IEnumerable<T> ResolveAll<T>();
    }
}