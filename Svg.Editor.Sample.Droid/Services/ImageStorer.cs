using System.IO;
using Android.Content;
using Svg.Droid.SampleEditor.Core.Interfaces;
using Svg.Platform;

namespace Svg.Droid.SampleEditor.Services
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