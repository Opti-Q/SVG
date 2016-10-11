using System;
using Svg;
using Svg.Core.Interfaces;
using Svg.Droid.Editor.Services;
using Svg.Interfaces;

[assembly: SvgService(typeof(ISvgCachingService), typeof(AndroidSvgCachingService))]

namespace Svg.Droid.Editor.Services
{
    public class AndroidSvgCachingService : ISvgCachingService
    {
        private readonly Func<string, ISvgSource> _sourceProvider;

        public AndroidSvgCachingService()
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

        /// <summary>
        /// This method is used to obtain the path where the cached PNG is saved.
        /// </summary>
        /// <param name="svgFilePath">The path to the SVG.</param>
        /// <param name="nameModifier">A name modifier for identifying variations of the cached PNG.</param>
        /// <param name="fs">Instance of the current filesystem.</param>
        /// <returns></returns>
        public string GetCachedPngPath(string svgFilePath, string nameModifier, IFileSystem fs)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(svgFilePath);
            return fs.PathCombine(fs.GetDefaultStoragePath(), $"{fileName}_{nameModifier}.png");
        }
    }
}