using System;
using System.Threading.Tasks;
using MvvmCross.Platform;
using MvvmCross.Plugins.PictureChooser;
using Svg.Core.Interfaces;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class AndroidPickImageService : IPickImageService
    {
        public async Task<string> PickImagePath()
        {
            using (var inStream = await Mvx.Resolve<IMvxPictureChooserTask>().ChoosePictureFromLibrary(2048, 80))
            {
                var fs = Engine.Resolve<IFileSystem>();
                var path = fs.PathCombine(fs.GetDefaultStoragePath(), $"{Guid.NewGuid()}.png");
                using (var outStream = fs.OpenWrite(path))
                {
                    inStream.CopyTo(outStream);
                }
                return path;
            }
        }
    }
}