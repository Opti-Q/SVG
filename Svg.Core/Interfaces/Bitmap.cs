
using System.Drawing;
using System.IO;
using Svg.Interfaces;

namespace Svg
{
    public interface Bitmap : Image
    {
        BitmapData LockBits(RectangleF rectangle, ImageLockMode lockmode, PixelFormat pixelFormat);
        void UnlockBits(BitmapData bitmapData);
        void SavePng(Stream stream, int quality = 100);
    }
}