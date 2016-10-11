using Android.Content.Res;
using Svg;
using Svg.Core.Interfaces;
using Svg.Droid.Editor.Services;
using Svg.Interfaces;
using Svg.Platform;

[assembly: SvgService(typeof(ISvgSourceFactory), typeof(AndroidSvgSourceFactory))]

namespace Svg.Droid.Editor.Services
{
    public class AndroidSvgSourceFactory : ISvgSourceFactory
    {
        private readonly AssetManager _assets;

        public AndroidSvgSourceFactory()
        {
            _assets = Android.App.Application.Context.Assets;
        }

        public ISvgSource Create(string path)
        {
            return new SvgAssetSource(path, _assets);
        }
    }
}