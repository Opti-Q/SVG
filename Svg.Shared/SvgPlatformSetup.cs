using System;
using Svg;
#if ANDROID
using Android.Content;
using Javax.Crypto;
#endif
using Svg.Interfaces;
using Svg.Platform;

[assembly:SvgPlatform(typeof(SvgPlatformSetup))]

namespace Svg
{

    public class SvgPlatformSetup : SvgPlatformSetupBase
    {
        private static bool _isInitialized = false;
        private static bool _enableFastTextRendering = true;

        public static bool EnableFastTextRendering
        {
            get { return _enableFastTextRendering; }
            set
            {
                _enableFastTextRendering = value;
                if (_enableFastTextRendering)
                {
#if ANDROID
                    Engine.Register<IAlternativeSvgTextRenderer, AndroidTextRenderer>(() => new AndroidTextRenderer());
#else
                    Engine.Register<IAlternativeSvgTextRenderer, SkiaTextRenderer>(() => new SkiaTextRenderer());
#endif
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

#if ANDROID
            Engine.Register<IFactory, IFactory>(() => new Factory());

            var ops = (SvgAndroidPlatformOptions)options;
            if (ops.EnableFastTextRendering)
            {
                Engine.Register<IAlternativeSvgTextRenderer, AndroidTextRenderer>(() => new AndroidTextRenderer());
            }
#else
            Engine.Register<IFactory, IFactory>(() => new SKFactory());
            
            if (EnableFastTextRendering)
            {
                Engine.Register<IAlternativeSvgTextRenderer, SkiaTextRenderer>(() => new SkiaTextRenderer());
            }
#endif
        }
    }
}