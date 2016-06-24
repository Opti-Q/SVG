using System;
using Android.Graphics;

namespace Svg.Platform
{
    public class AndroidTextureBrush : AndroidBrushBase, TextureBrush, IDisposable
    {
        private AndroidBitmap _image;
        private BitmapShader _shader;

        public AndroidTextureBrush(AndroidBitmap image)
        {
            _image = image;
        }

        public override void Dispose()
        {
            base.Dispose();
            _image?.Dispose();
            _image = null;
            _shader?.Dispose();
            _shader = null;
        }
        // TODO LX what about Transform?
        public Matrix Transform { get; set; }

        protected override Paint CreatePaint()
        {
            var paint = new Paint();
            if (_shader != null)
            {
                _shader.Dispose();
                _shader = null;
            }

            _shader = new BitmapShader(_image.Image, Shader.TileMode.Clamp, Shader.TileMode.Clamp);

            paint.SetShader(_shader);
            return paint;
        }
    }
}