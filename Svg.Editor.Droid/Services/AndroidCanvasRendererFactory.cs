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
            var c = new Canvas(androidBitmap.Image);
            return new AndroidCanvasRenderer(c);
        }
    }
}