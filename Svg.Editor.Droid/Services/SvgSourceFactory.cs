using Android.Content.Res;
using Svg;
using Svg.Core.Interfaces;
using Svg.Droid.Editor.Services;
using Svg.Interfaces;
using Svg.Platform;

[assembly: SvgService(typeof(ISvgSourceFactory), typeof(SvgSourceFactory))]

namespace Svg.Droid.Editor.Services
{
    public class SvgSourceFactory : ISvgSourceFactory
    {
        private readonly AssetManager _assets;

        public SvgSourceFactory()
        {
            _assets = Android.App.Application.Context.Assets;
        }

        public ISvgSource Create(string path)
        {
            return new SvgAssetSource(path, _assets);
        }
    }
}