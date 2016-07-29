using Svg.Interfaces;
using Svg.Platform;

namespace Svg
{
    public class SvgAndroidPlatformOptions : SvgPlatformOptions
    {
        public bool EnableFastTextRendering { get; set; } = true;
    }

    public class SvgPlatformSetup : SvgPlatformSetupBase
    {
        private static bool _isInitialized = false;

        protected override void Initialize(SvgPlatformOptions options)
        {
            base.Initialize(options);

            Engine.Register<IFactory, IFactory>(() => new SKFactory());
            var ops = (SvgAndroidPlatformOptions)options;
            if (ops.EnableFastTextRendering)
            {
                Engine.Register<IAlternativeSvgTextRenderer, SkiaTextRenderer>(() => new SkiaTextRenderer());
            }
        }

        public static void Init(SvgAndroidPlatformOptions options)
        {
            if (_isInitialized)
                return;

            new SvgPlatformSetup().Initialize(options);

            _isInitialized = true;
        }
    }
}