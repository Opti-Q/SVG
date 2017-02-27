using System.Reactive.Concurrency;
using System.Threading;
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
        private static bool _enableFastTextRendering = true;

        public static bool EnableFastTextRendering
        {
            get { return _enableFastTextRendering; }
            set
            {
                _enableFastTextRendering = value;
                if (_enableFastTextRendering)
                {
                    Engine.Register<IAlternativeSvgTextRenderer, SkiaTextRenderer>(() => new SkiaTextRenderer());
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Engine.Register<IFactory, IFactory>(() => new SKFactory());
            var context = SynchronizationContext.Current;
            if (context != null)
            {
                var scheduler = new SynchronizationContextScheduler(context);
                Engine.Register<IScheduler, SynchronizationContextScheduler>(() => scheduler);
            }

            if (EnableFastTextRendering)
            {
                Engine.Register<IAlternativeSvgTextRenderer, SkiaTextRenderer>(() => new SkiaTextRenderer());
            }
        }
    }
}