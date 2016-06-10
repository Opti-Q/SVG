
using System.Drawing;
using Svg.Interfaces;

namespace Svg
{
    public interface Bitmap : Image
    {
        BitmapData LockBits(RectangleF rectangle, ImageLockMode lockmode, PixelFormat pixelFormat);
        void UnlockBits(BitmapData bitmapData);
        void Save(string path);
    }
}