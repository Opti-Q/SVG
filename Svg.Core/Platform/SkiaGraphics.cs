using System;
using System.Threading;
using SkiaSharp;

namespace Svg.Platform
{
    public class SkiaGraphics : Graphics
    {
        private readonly SKSurface _surface;
        private readonly SKCanvas _canvas;
        private SkiaMatrix _matrix;
        private Region _clip;

        public SkiaGraphics(SkiaBitmap image)
        {
            _surface = SKSurface.Create(image.Image.Info);
            _canvas = _surface.Canvas;
            _matrix = new SkiaMatrix(_canvas.TotalMatrix);
        }

        public SkiaGraphics(SKSurface surface)
        {
            _surface = surface;
            _canvas = _surface.Canvas;
            _matrix = new SkiaMatrix(_canvas.TotalMatrix);
        }

        // TODO LX use textrenderinghint
        public TextRenderingHint TextRenderingHint { get; set; }

        public float DpiY
        {
            get
            {
                throw new NotSupportedException("SKiaSharp does not support DpiY or Density");
            }
        }

        public Region Clip { get { return _clip; } }

        // TODO LX use smootingmode
        public SmoothingMode SmoothingMode { get; set; }

        public void DrawImage(Bitmap bitmap, Interfaces.RectangleF rectangle, int x, int y, int width, int height, GraphicsUnit pixel)
        {
            var img = (SkiaBitmap) bitmap;
            _canvas.DrawBitmap(img.Image, new SKRect(x, y, x+width,y+height));
        }

        public void DrawImage(Bitmap bitmap, Interfaces.RectangleF rectangle, int x, int y, int width, int height, GraphicsUnit pixel, ImageAttributes attributes)
        {
            throw new NotImplementedException("ImageAttributes not implemented for now: see http://chiuki.github.io/android-shaders-filters/#/");
            //var img = (AndroidBitmap)bitmap;
            //_canvas.DrawBitmap(img.Image, null, new Rect(x, y, x + width, y + height), null);
        }

        public void DrawImage(Image bitmap, Interfaces.RectangleF destRect, Interfaces.RectangleF srcRect, GraphicsUnit graphicsUnit)
        {
            var img = (SkiaBitmap) bitmap;

            var src = (SKRect)(SkiaRectangleF)srcRect;
            var dest = (SKRect)(SkiaRectangleF)destRect;

            _canvas.DrawBitmap(img.Image, src, dest, null);
        }

        public void DrawImageUnscaled(Image image, Svg.Interfaces.PointF location)
        {
            var img = (SkiaBitmap)image;
            _canvas.DrawBitmap(img.Image, (int)location.X, (int)location.Y, null);
        }

        public void DrawImage(Image image, Interfaces.PointF location)
        {
            var img = (SkiaBitmap)image;
            _canvas.DrawBitmap(img.Image, (int)location.X, (int)location.Y, null);
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            var p = (SkiaGraphicsPath) path;
            var paint = (SkiaPen)pen;
            paint.Paint.IsStroke = true;
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
            var p = (SkiaGraphicsPath)path;

            var b = (SkiaBrushBase) brush;
            
            b.Paint.IsStroke = false;
            SetSmoothingMode(b.Paint);
                
            _canvas.DrawPath(p.Path, b.Paint);
            
        }

        public void DrawText(string text, float x, float y, Pen pen)
        {
            if (text == null)
                return;
            var paint = (SkiaPen)pen;
            _canvas.DrawText(text, x, y, paint.Paint);
        }

        private void SetSmoothingMode(SKPaint paint)
        {
            //switch (SmoothingMode)
            //{
            //    case SmoothingMode.Default:
            //    case SmoothingMode.None:
            //        paint.Flags = 0;
            //        break;
            //    case SmoothingMode.AntiAlias:
            //        paint.Flags |= PaintFlags.AntiAlias;
            //        break;
            //    //case SmoothingMode.HighQuality:
            //    //case SmoothingMode.HighSpeed:
            //    //case SmoothingMode.Invalid:
            //}
        }

        public void SetClip(Region region, CombineMode combineMode)
        {
            var op = SKRegionOperation.Union;
            switch (combineMode)
            {
                case CombineMode.Complement:
                    // TODO LX is this correct?
                    op = SKRegionOperation.ReverseDifference;
                    break;
                case CombineMode.Exclude:
                    // TODO LX is this correct?
                    op = SKRegionOperation.Difference;
                    break;
                case CombineMode.Intersect:
                    op = SKRegionOperation.Intersect;
                    break;
                case CombineMode.Replace:
                    op = SKRegionOperation.Replace;
                    break;
                case CombineMode.Union:
                    op = SKRegionOperation.Union;
                    break;
                case CombineMode.Xor:
                    op = SKRegionOperation.XOR;
                    break;
            }
            _clip = region;
            //if (region != null)
            //    _canvas.ClipRect((SkiaRectangleF) region.Rect, op);
            //else
            //{
            //    SKRect r = new SKRect();
            //    _canvas.GetClipBounds(ref r);
            //    _canvas.ClipRect(r, SKRegionOperation.Union);
            //}
        }

        public Region[] MeasureCharacterRanges(string text, Font font, Interfaces.RectangleF rectangle, StringFormat format)
        {
            // TODO LX: wtf?
            //throw new NotImplementedException();
            return new[] {new Region(rectangle)};
        }

        public Matrix Transform
        {
            get { return (SkiaMatrix)_canvas.TotalMatrix; }
            set
            {
                _matrix = (SkiaMatrix)value;
                _canvas.SetMatrix(_matrix.Matrix);
            }
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
            {
                //_canvas.Translate(dx, dy);
                throw new NotSupportedException("Skiasharp does not support that yet");
            }
            else
            {
                _canvas.Translate(dx, dy);
            }
        }

        public void RotateTransform(float fAngle, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
            {
                //_canvas.Matrix.PostRotate(fAngle);
                throw new NotSupportedException("Skiasharp does not support that yet");
            }
            else
            {
                _canvas.RotateDegrees(fAngle);
            }
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            if (order == MatrixOrder.Append)
            {
                //_canvas.Matrix.PostScale(sx, sy);
                throw new NotSupportedException("Skiasharp does not support that yet");
            }
            else
            {
                _canvas.Scale(sx, sy);
            }
        }

        public void Concat(Matrix matrix)
        {
            var m = ((SkiaMatrix) matrix).Matrix;
            _canvas.Concat(ref m);
        }

        public void FillBackground(Svg.Interfaces.Color color)
        {
            var c = (SkiaColor)color;
            _canvas.DrawColor(c);
        }

        public void Flush()
        {
            throw new NotSupportedException("Flushing not supported on android");
        }

        public void Save()
        {
            _canvas.Save();
        }

        public void Restore()
        {
            _canvas.Restore();
        }

        public void Dispose()
        {
            _surface.Dispose();
        }
    }
}