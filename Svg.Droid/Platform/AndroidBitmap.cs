using System;
using RectangleF = Svg.Interfaces.RectangleF;

namespace Svg.Platform
{

    public class AndroidBitmap : Svg.Bitmap
    {
        protected Android.Graphics.Bitmap _image;

        public AndroidBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            
            _image = Android.Graphics.Bitmap.CreateBitmap(width, height, Android.Graphics.Bitmap.Config.Argb8888);
        }

        public AndroidBitmap(Image inputImage)
        {
            var ii = (AndroidBitmap) inputImage;
            _image = Android.Graphics.Bitmap.CreateBitmap(ii._image);
        }

        public AndroidBitmap(Android.Graphics.Bitmap bitmap)
        {
            _image = bitmap;
        }

        protected AndroidBitmap()
        {

        }

        public Android.Graphics.Bitmap Image
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
            using(var fn = System.IO.File.OpenWrite(path))
                _image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, fn); // bmp is your Bitmap instance
            // PNG is a lossless format, the compression factor (100) is ignored
        }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}