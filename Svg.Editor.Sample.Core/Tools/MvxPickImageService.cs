﻿using System.Threading.Tasks;
using MvvmCross.Platform;
using MvvmCross.Plugins.PictureChooser;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class MvxPickImageService : IPickImageService
    {
        public async Task<string> PickImagePathAsync(int maxPixelDimension)
        {
            using (var inStream = await Mvx.Resolve<IMvxPictureChooserTask>().ChoosePictureFromLibrary(maxPixelDimension, 80))
            {
                if (inStream == null) return null;

                var fs = SvgEngine.Resolve<IFileSystem>();
	            var path = "background.png";

				var fullPath = fs.PathCombine(fs.GetDefaultStoragePath(), path);

                if (fs.FileExists(fullPath))
                    fs.DeleteFile(fullPath);

                using (var outStream = fs.OpenWrite(fullPath))
                {
                    inStream.CopyTo(outStream);
                }
                return path;
            }
        }
    }
}