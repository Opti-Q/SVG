using System.IO;
using Svg.Core.Interfaces;
using Svg.Platform;

namespace Svg.Droid.Editor.Services
{
    public class ImageStorer: IImageStorer
    {
        public void SaveAsPng(Bitmap image, Stream stream)
        {
            var bitmap = (AndroidBitmap) image;

            bitmap.Image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
        }
    }
}