using System;
using System.IO;
using SkiaSharp;
using Svg.Interfaces;
using RectangleF = Svg.Interfaces.RectangleF;

namespace Svg.Platform
{

    public class SkiaBitmap : Svg.Bitmap
    {
        protected SKBitmap _image;

        public SkiaBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            
            _image = new SKBitmap(width, height, SKColorType.Rgba_8888, SKAlphaType.Opaque);
        }

        public SkiaBitmap(Image inputImage)
        {
            var ii = (SkiaBitmap) inputImage;
            _image = new SKBitmap(ii._image.Info);
        }

        public SkiaBitmap(SKBitmap bitmap)
        {
            _image = bitmap;
        }

        protected SkiaBitmap()
        {

        }

        public SKBitmap Image
        {
            get { return _image; }
        }

        public void Dispose()
        {
            _image.Dispose();
        }

        public BitmapData LockBits(RectangleF rectangle, ImageLockMode lockmode, PixelFormat pixelFormat)
        {
            throw new NotImplementedException();
        }

        public void UnlockBits(BitmapData bitmapData)
        {
            _image.UnlockPixels();
        }

        public void SavePng(Stream stream, int quality = 100)
        {
            SKBitmap bitmap = Image;
            try
            {
                bitmap.LockPixels();
                IntPtr p;
                bitmap.GetPixels(out p);
                using (var img = SKImage.FromPixels(bitmap.Info, p, bitmap.Width * bitmap.BytesPerPixel))
                {
                    var data = img.Encode(SKImageEncodeFormat.Png, quality: quality);
                    data.SaveTo(stream);
                }
            }
            finally
            {
                bitmap.UnlockPixels();
            }
        }


        public int Width { get; set; }
        public int Height { get; set; }
    }
}