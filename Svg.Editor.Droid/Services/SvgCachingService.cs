using System;
using System.Linq;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Droid.Editor.Services
{
    public class SvgCachingService
    {
        public void SetupSvgCache(ITool tool, Func<string, ISvgSource> sourceProvider)
        {
            // load svg from FS
            var colorTool = tool as ColorTool;
            if (colorTool == null) return;
            var provider = sourceProvider($"svg/{colorTool.ColorIconName}");
            var document = SvgDocument.Open<SvgDocument>(provider);
            var fs = Engine.Resolve<IFileSystem>();


            foreach (var selectableColor in colorTool.SelectableColors)
            {
                // apply changes to svg
                document.Children.Single().Children.Last().Fill = new SvgColourServer(selectableColor);

                // save svg as png
                using (var bmp = document.DrawAllContents(Engine.Factory.Colors.Transparent))
                {
                    // now save it as PNG
                    //var path = fs.PathCombine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)
                    //    .AbsolutePath, $"icon_{selectableColor.R}_{selectableColor.G}_{selectableColor.B}.png");
                    var path = fs.PathCombine(fs.GetDefaultStoragePath(), $"icon_{selectableColor.R}_{selectableColor.G}_{selectableColor.B}.png");
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
}