using System;
using System.IO;
using SkiaSharp;
using Svg.Core.Interfaces;

namespace Svg.Droid.Editor.Services
{
    public class ImageStorer: IImageStorer
    {
        public void SaveAsPng(Bitmap image, Stream stream)
        {
#if !SKIA
            var bitmap = (AndroidBitmap) image;

            bitmap.Image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
#else
            throw new NotSupportedException("Not supported by SKIA at the moment");
#endif
        }
    }
}