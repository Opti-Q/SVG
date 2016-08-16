using Android.Content.Res;
using Svg.Core.Interfaces;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg.Droid.Editor.Services
{
    public class SvgSourceFactory : ISvgSourceFactory
    {
        private readonly AssetManager _assets;

        public SvgSourceFactory(AssetManager assets)
        {
            _assets = assets;
        }

        public ISvgSource Create(string path)
        {
            return new SvgAssetSource(path, _assets);
        }
    }
}