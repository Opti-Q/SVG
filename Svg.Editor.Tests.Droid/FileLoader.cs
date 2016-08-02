using Android.Content.Res;
using Svg.Platform;

namespace Svg.Editor.Tests
{
    internal class FileLoader : IFileLoader
    {
        private readonly AssetManager _assets;

        public FileLoader(AssetManager assets)
        {
            _assets = assets;
        }

        public SvgDocument Load(string fileName)
        {
            var src = new SvgAssetSource(fileName, _assets);
            return SvgDocument.Open<SvgDocument>(src);
        }
    }
}