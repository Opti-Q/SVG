using System;
using Svg.Core.Interfaces;
using Svg.Interfaces;

namespace Svg.Droid.Editor.Services
{
    public class SvgCachingService : ISvgCachingService
    {
        private readonly Func<string, ISvgSource> _sourceProvider;

        public SvgCachingService(Func<string, ISvgSource> sourceProvider)
        {
            _sourceProvider = sourceProvider;
        }

        public void SaveAsPng(string sourceName, string name, Action<SvgDocument> preprocessAction = null)
        {
            // load svg from FS
            var document = SvgDocument.Open<SvgDocument>(_sourceProvider(sourceName));
            var fs = Engine.Resolve<IFileSystem>();

            // apply changes to svg
            preprocessAction?.Invoke(document);

            // save svg as png
            using (var bmp = document.DrawAllContents(Engine.Factory.Colors.Transparent))
            {
                // now save it as PNG
                var path = fs.PathCombine(fs.GetDefaultStoragePath(), name);
                if (fs.FileExists(path))
                    fs.DeleteFile(path);

                using (var stream = fs.OpenWrite(path))
                {
                    bmp.SavePng(stream);
                }
            }
        }
    }
}