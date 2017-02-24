
using System.IO;
using Svg.Interfaces;

namespace Svg
{
    public abstract class Bitmap : Image
    {
        public static Bitmap Create(int width, int height)
        {
            return Engine.Factory.CreateBitmap(width, height);
        }

        public static Bitmap Create(Image image)
        {
            return Engine.Factory.CreateBitmap(image);
        }

        public abstract BitmapData LockBits(RectangleF rectangle, ImageLockMode lockmode, PixelFormat pixelFormat);
        public abstract void UnlockBits(BitmapData bitmapData);
        public abstract void SavePng(Stream stream, int quality = 100);
        public abstract void Dispose();

        public abstract int Width { get; protected set; }
        public abstract int Height { get; protected set; }
    }
}