﻿using System.Threading;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg
{
    public static class SvgPlatform
    {
        private static bool _initialized = false;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        public static void Init(SvgPlatformOptions options = null)
        {
            if (_initialized)
                return;

            try
            {
                _lock.Wait();

                if (_initialized)
                    return;
#if !PCL
                if (options?.ServiceLocator != null)
                    SvgEngine.ServiceLocator = options.ServiceLocator;
                // register base services
                SvgEngine.RegisterSingleton<IMarshal>(() => new SvgMarshal());
                SvgEngine.RegisterSingleton<ISvgElementAttributeProvider>(
                    () => new SvgElementAttributeProvider());
                SvgEngine.RegisterSingleton<ICultureHelper>(() => new CultureHelper());
                SvgEngine.RegisterSingleton<ILogger>(() => new DefaultLogger());
                SvgEngine.RegisterSingleton<ICharConverter>(() => new SvgCharConverter());
                SvgEngine.Register<IWebRequest>(() => new WebRequestSvc());
                SvgEngine.RegisterSingleton<IFileSystem>(() => new FileSystem());
                SvgEngine.Register<IAlternativeSvgTextRenderer>(() => new SkiaTextRenderer());
                SvgEngine.Register<ISvgCachingService>(() => new SvgCachingService());
                // register platform specific services

                SvgEngine.Register<IFactory>(() => new SKFactory());
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
