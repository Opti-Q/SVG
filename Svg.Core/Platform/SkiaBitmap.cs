using System;
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

        public void Save(string path)
        {
            var fs = Engine.Resolve<IFileSystem>();

            //using(var fn = fs.OpenWrite(path))
            //    _image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, fn); // bmp is your Bitmap instance
            //// PNG is a lossless format, the compression factor (100) is ignored
            throw new NotImplementedException("Save not implemented");
        }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}