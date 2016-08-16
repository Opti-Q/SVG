using System;
using Android.Content;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Droid.Editor.Services;
using Svg.Interfaces;

namespace Svg.Droid.Editor
{
    public static class SvgEditor
    {
        public static void Init(Context context)
        {
            // Initialize SVG Platform and tie together PCL and platform specific modules
            SvgPlatformSetup.Init(new SvgSkiaPlatformOptions() { EnableFastTextRendering = true });

            Engine.Register<ITextInputService, TextInputService>(() => new TextInputService(context));
            Engine.Register<IColorInputService, ColorInputService>(() => new ColorInputService(context));
            Engine.Register<ILineOptionsInputService, LineOptionsInputService>(() => new LineOptionsInputService(context));
            Engine.Register<ISvgSourceFactory, SvgSourceFactory>(() => new SvgSourceFactory(context.Assets));
            Func<string, ISvgSource> svgSourceProvider = source => Engine.Resolve<ISvgSourceFactory>().Create(source);
            Engine.Register<ISvgCachingService, SvgCachingService>(() => new SvgCachingService(svgSourceProvider));
        }
    }
}