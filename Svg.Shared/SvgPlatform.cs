using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Svg.Interfaces;

namespace Svg
{
    public static class SvgPlatform
    {
        private static bool _initialized = false;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        public static void Init()
        {
            if (_initialized)
                return;

            try
            {
                _lock.Wait();

                if (_initialized)
                    return;
#if !PCL
                // register base services
                Engine.RegisterSingleton<IMarshal, SvgMarshal>(() => new SvgMarshal());
                Engine.RegisterSingleton<ISvgElementAttributeProvider, SvgElementAttributeProvider>(
                    () => new SvgElementAttributeProvider());
                Engine.RegisterSingleton<ICultureHelper, CultureHelper>(() => new CultureHelper());
                Engine.RegisterSingleton<ILogger, DefaultLogger>(() => new DefaultLogger());
                Engine.RegisterSingleton<ICharConverter, SvgCharConverter>(() => new SvgCharConverter());
                Engine.Register<IWebRequest, WebRequestSvc>(() => new WebRequestSvc());
                Engine.RegisterSingleton<IFileSystem, FileSystem>(() => new FileSystem());

                // register platform specific services

                Engine.Register<IFactory, IFactory>(() => new SKFactory());
#endif
                _initialized = true;
            }
            finally
            {
                _lock.Release();
            }

            return;
        }
    }
}
