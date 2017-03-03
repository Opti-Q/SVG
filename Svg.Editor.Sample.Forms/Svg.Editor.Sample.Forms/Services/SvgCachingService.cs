using System;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Editor.Sample.Forms.Services
{
    public class SvgCachingService : ISvgCachingService
    {
        private readonly Func<string, ISvgSource> _sourceProvider;

        public SvgCachingService()
        {
            _sourceProvider = (src) => Engine.Resolve<ISvgSourceFactory>().Create(src);
        }

        public void SaveAsPng(string svgFilePath, string nameModifier, Action<SvgDocument> preprocessAction)
        {
            // load svg from FS
            var document = SvgDocument.Open<SvgDocument>(_sourceProvider(svgFilePath));

            // apply changes to svg
            preprocessAction?.Invoke(document);

            // save svg as png
            using (var bmp = document.DrawAllContents(Engine.Factory.Colors.Transparent))
            {
                var fs = Engine.Resolve<IFileSystem>();
                var path = GetCachedPngPath(svgFilePath, nameModifier, fs);
                if (fs.FileExists(path))
                    fs.DeleteFile(path);

                using (var stream = fs.OpenWrite(path))
                {
                    bmp.SavePng(stream);
                }
            }
        }

        public string GetCachedPngPath(string svgFilePath, string nameModifier, IFileSystem fs)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(svgFilePath);
            return fs.PathCombine(fs.GetDefaultStoragePath(), "SvgCache", $"{fileName}_{nameModifier}.png");
        }
    }
}
