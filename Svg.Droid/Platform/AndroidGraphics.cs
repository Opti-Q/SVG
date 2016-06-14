using System;
using System.Drawing;
using System.Linq;
using Android.Graphics;

namespace Svg.Platform
{
    public class AndroidGraphics : Graphics
    {
        private readonly Canvas _canvas;
        private AndroidMatrix _matrix;
        private Region _clip;

        public AndroidGraphics(AndroidBitmap image)
        {
            _canvas = new Canvas(image.Image);
            _matrix = new AndroidMatrix(_canvas.Matrix);
        }

        public AndroidGraphics(Canvas canvas)
        {
            _canvas = canvas;
            _matrix = new AndroidMatrix(_canvas.Matrix);
        }

        // TODO LX use textrenderinghint
        public TextRenderingHint TextRenderingHint { get; set; }
        public float DpiY { get { return (float)_canvas.Density; } }
        public Region Clip { get { return _clip; } }
        // TODO LX use smootingmode
        public SmoothingMode SmoothingMode { get; set; }
        public Matrix Transform
        {
            get { return _matrix; }
            set
            {
                _matrix = (AndroidMatrix)value;
                _canvas.Matrix = _matrix.Matrix;
            }
        }
        public void Dispose()
        {
            _canvas.Dispose();
        }
        public void DrawImage(Bitmap bitmap, Interfaces.RectangleF rectangle, int x, int y, int width, int height, GraphicsUnit pixel)
        {
            var img = (AndroidBitmap) bitmap;
            _canvas.DrawBitmap(img.Image, null, new Rect(x, y, x+width,y+height), null);
        }
        public void DrawImage(Bitmap bitmap, Interfaces.RectangleF rectangle, int x, int y, int width, int height, GraphicsUnit pixel,
            ImageAttributes attributes)
        {
            throw new NotImplementedException("ImageAttributes not implemented for now: see http://chiuki.github.io/android-shaders-filters/#/");
            //var img = (AndroidBitmap)bitmap;
            //_canvas.DrawBitmap(img.Image, null, new Rect(x, y, x + width, y + height), null);
        }
        public void DrawImage(Image bitmap, Interfaces.RectangleF destRect, Interfaces.RectangleF srcRect, GraphicsUnit graphicsUnit)
        {
            var img = (AndroidBitmap) bitmap;

            var src = (Rect)(AndroidRectangleF)srcRect;
            var dest = (Rect)(AndroidRectangleF)destRect;

            _canvas.DrawBitmap(img.Image, src, dest, null);
        }
        public void DrawImageUnscaled(Image image, Svg.Interfaces.PointF location)
        {
            var img = (AndroidBitmap)image;
            _canvas.DrawBitmap(img.Image, (int)location.X, (int)location.Y, null);
        }
        public void DrawImage(Image image, Interfaces.PointF location)
        {
            var img = (AndroidBitmap)image;
            _canvas.DrawBitmap(img.Image, (int)location.X, (int)location.Y, null);
        }
        public void Flush()
        {
            throw new NotSupportedException("Flushing not supported on android");
        }
        public void Save()
        {
            _canvas.Save();
        }
        public void DrawPath(Pen pen, GraphicsPath path)
        {
            var p = (AndroidGraphicsPath) path;
            var paint = (AndroidPen)pen;
            paint.Paint.SetStyle(Paint.Style.Stroke);
            SetSmoothingMode(paint.Paint);
            
            _canvas.DrawPath(p.Path, paint.Paint);

            // little hack as android path does not support text!
            foreach (var text in p.Texts)
            {
                _canvas.DrawText(text.text, text.location.X, text.location.Y, paint.Paint);
            }

        }
        public void FillPath(Brush brush, GraphicsPath path)
        {
            var p = (AndroidGraphicsPath)path;

            var shader = (IAndroidShader) brush;

            using (var paint = new Paint())
            {
                //paint.StrokeWidth = 1;
                paint.SetStyle(Paint.Style.Fill);
                shader.ApplyTo(paint);
                SetSmoothingMode(paint);
                _canvas.DrawPath(p.Path, paint);
            }
        }
        private void SetSmoothingMode(Paint paint)
        {
            switch (SmoothingMode)
            {
                case SmoothingMode.Default:
                case SmoothingMode.None:
                    paint.Flags = 0;
                    break;
                case SmoothingMode.AntiAlias:
                    paint.Flags |= PaintFlags.AntiAlias;
                    break;
                //case SmoothingMode.HighQuality:
                //case SmoothingMode.HighSpeed:
                //case SmoothingMode.Invalid:
            }
        }
        public void RotateTransform(float fAngle, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
            {
                _canvas.Matrix.PostRotate(fAngle);
            }
            else
            {
                _canvas.Matrix.PreRotate(fAngle);
            }
        }
        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
            {
                _canvas.Matrix.PostScale(sx, sy);
            }
            else
            {
                _canvas.Matrix.PreScale(sx, sy);
            }
        }
        public void SetClip(Region region, CombineMode combineMode)
        {
            var op = Android.Graphics.Region.Op.Union;
            switch (combineMode)
            {
                case CombineMode.Complement:
                    // TODO LX is this correct?
                    op = Android.Graphics.Region.Op.ReverseDifference;
                    break;
                case CombineMode.Exclude:
                    // TODO LX is this correct?
                    op = Android.Graphics.Region.Op.Difference;
                    break;
                case CombineMode.Intersect:
                    op = Android.Graphics.Region.Op.Intersect;
                    break;
                case CombineMode.Replace:
                    op = Android.Graphics.Region.Op.Replace;
                    break;
                case CombineMode.Union:
                    op = Android.Graphics.Region.Op.Union;
                    break;
                case CombineMode.Xor:
                    op = Android.Graphics.Region.Op.Xor;
                    break;
            }
            _clip = region;

            if(region != null)
                _canvas.ClipRect((Rect)(AndroidRectangleF)region.Rect, op);
            else
                _canvas.ClipRect(_canvas.ClipBounds, Android.Graphics.Region.Op.Union);
        }
        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
            {
                _canvas.Matrix.PostTranslate(dx, dy);
            }
            else
            {
                _canvas.Matrix.PreTranslate(dx, dy);
            }
        }
        public Region[] MeasureCharacterRanges(string text, Font font, Interfaces.RectangleF rectangle, StringFormat format)
        {
            // TODO LX: wtf?
            //throw new NotImplementedException();
            return new[] {new Region(rectangle)};
        }

        public void DrawText(string text, float x, float y, Pen pen)
        {
            if (text == null)
                return;
            var paint = (AndroidPen)pen;
            _canvas.DrawText(text, x, y, paint.Paint);
        }

        public Region[] MeasureCharacterRanges(string text, Font font, Rectangle rectangle, StringFormat format)
        {
            // TODO LX: wtf?
            return new[] { new Region(Engine.Factory.CreateRectangleF(rectangle.X, rectangle.Y,rectangle.Width, rectangle.Height)) };
        }
    }
}