﻿using System.Threading;
using Svg.Editor.Interfaces;

namespace Svg.Editor
{
    public static class SvgEditorViews
    {
        private static bool _initialized;
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

                SvgPlatform.Init();
                SvgEditor.Init();


#if WINDOWS_UWP
                Engine.Register<IToolbarIconSizeProvider, Svg.Editor.Views.UWP.UWPToolbarIconSizeProvider>(() => new Svg.Editor.Views.UWP.UWPToolbarIconSizeProvider());
#elif __IOS__
                Engine.Register<IToolbarIconSizeProvider, Svg.Editor.Views.iOS.TouchToolbarIconSizeProvider>(() => new Svg.Editor.Views.iOS.TouchToolbarIconSizeProvider());
#elif PLATFORM_ANDROID
                Engine.Register<IToolbarIconSizeProvider, Svg.Editor.Views.Droid.AndroidToolbarIconSizeProvider>(() => new Svg.Editor.Views.Droid.AndroidToolbarIconSizeProvider());
#endif

                _initialized = true;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
