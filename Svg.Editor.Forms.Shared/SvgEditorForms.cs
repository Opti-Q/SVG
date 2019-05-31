using System.Threading;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Interfaces;

namespace Svg.Editor.Forms
{
    public static class SvgEditorForms
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


                _initialized = true;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}