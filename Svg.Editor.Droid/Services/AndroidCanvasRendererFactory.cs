using Android.Graphics;
using Svg.Core.Interfaces;
using Svg.Platform;

namespace Svg.Droid.Editor.Services
{
    public class AndroidCanvasRendererFactory : IRendererFactory
    {
        public IRenderer Create(Bitmap bitmap)
        {
            var androidBitmap = (AndroidBitmap) bitmap;
            return new AndroidCanvasRenderer(new Canvas(androidBitmap.Image));
        }
    }
}