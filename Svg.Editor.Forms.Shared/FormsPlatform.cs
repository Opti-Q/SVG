using Svg.Editor.Forms.Services;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Forms
{
    public static class FormsPlatform
    {
        public static void Init()
        {
            Engine.Register<ISvgSourceFactory, EmbeddedResourceSvgSourceFactory>(() => new EmbeddedResourceSvgSourceFactory());
        }
    }
}
