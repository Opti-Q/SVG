using System;
using System.IO;
using Svg;
using Svg.Editor.Interfaces;
using Svg.Editor.Views.Droid.Services;
using Svg.Interfaces;

[assembly: SvgService(typeof(ISvgCachingService), typeof(AndroidSvgCachingService))]
namespace Svg.Editor.Views.Droid.Services
{
    public class AndroidSvgCachingService : ISvgCachingService
    {
        private readonly Func<string, ISvgSource> _sourceProvider;

        private Lazy<IFileSystem> _fs = new Lazy<IFileSystem>(() => SvgEngine.Resolve<IFileSystem>());

        public AndroidSvgCachingService()
        {
            _sourceProvider = path => SvgEngine.Resolve<ISvgSourceFactory>().Create(path);
        }

        public string GetCachedPng(string svgFilePath, SaveAsPngOptions options)
        {
            if (svgFilePath == null) throw new ArgumentNullException(nameof(svgFilePath));
            if (options == null) throw new ArgumentNullException(nameof(options));

            // load svg from FS
            var document = SvgDocument.Open<SvgDocument>(_sourceProvider(svgFilePath));

            // apply changes to svg
            options.PreprocessAction?.Invoke(document);

            var dimension = GetDimension(options);

            // save svg as png
            using (var bmp = SvgEngine.Factory.CreateBitmap((int)dimension.Width, (int)dimension.Height))
            {
                document.DrawAllContents(bmp, options.BackgroundColor);
                var fs = _fs.Value;
                var path = GetCachedPngPath(svgFilePath, options);
                if (fs.FileExists(path))
                {
                    // if re-creation is forced
                    if (options.Force)
                        fs.DeleteFile(path);
                    else
                        return path;
                }

                using (var stream = fs.OpenWrite(path))
                {
                    bmp.SavePng(stream);
                }

                return path;
            }
        }

        public void Clear()
        {
            var fs = _fs.Value;
            var dirPath = fs.PathCombine(fs.GetDefaultStoragePath(), "SvgCache");
            if (fs.FolderExists(dirPath))
                fs.DeleteFolder(dirPath);
        }

        private string GetCachedPngPath(string svgFilePath, SaveAsPngOptions options)
        {
            var fs = _fs.Value;
            var dim = GetDimension(options);
            fs.EnsureDirectoryExists(fs.PathCombine(fs.GetDefaultStoragePath(), "SvgCache"));
            var filename = options.NamingConvention(svgFilePath, options) ?? $"{Path.GetFileNameWithoutExtension(svgFilePath)}_{(int)dim.Width}px_{(int)dim.Height}px.png";
            return fs.PathCombine(fs.GetDefaultStoragePath(), "SvgCache", filename);
        }

        private static SizeF GetDimension(SaveAsPngOptions options)
        {
            return options.ImageDimension ?? SizeF.Create(120, 120);
        }
    }
}