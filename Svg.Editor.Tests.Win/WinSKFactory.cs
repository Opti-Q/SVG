using Svg;
using Svg.Editor.Tests;
using Svg.Interfaces;

[assembly: SvgService(typeof(IFactory), typeof(WinSKFactory))]

namespace Svg.Editor.Tests
{
    public class WinSKFactory : SKFactory
    {
        public override Graphics CreateGraphicsFromImage(Bitmap input)
        {
            return new DummyGraphics();
        }

        public override Graphics CreateGraphicsFromImage(Image image)
        {
            return new DummyGraphics();
        }

        private class DummyGraphics : Graphics
        {
            public void Dispose()
            {
                
            }

            public void DrawImage(Bitmap bitmap, RectangleF rectangle, int x, int y, int width, int height, GraphicsUnit pixel)
            {
                
            }

            public void DrawImage(Bitmap bitmap, RectangleF rectangle, int x, int y, int width, int height, GraphicsUnit pixel,
                ImageAttributes attributes)
            {
                
            }

            public void Flush()
            {
                
            }

            public void Save()
            {
                
            }

            public void Restore()
            {
                
            }

            public TextRenderingHint TextRenderingHint { get; set; }
            public float DpiY { get; }
            public Region Clip { get; }
            public SmoothingMode SmoothingMode { get; set; }
            public Matrix Transform { get; set; }
            public void DrawImage(Image bitmap, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
            {
                
            }

            public void DrawImageUnscaled(Image image, PointF location)
            {
                
            }

            public void DrawPath(Pen pen, GraphicsPath path)
            {
                
            }

            public void FillPath(Brush brush, GraphicsPath path)
            {
                
            }

            public void RotateTransform(float fAngle, MatrixOrder order)
            {
                
            }

            public void ScaleTransform(float sx, float sy, MatrixOrder order)
            {
                
            }

            public void DrawImage(Image image, PointF location)
            {
                
            }

            public void SetClip(Region region, CombineMode combineMode)
            {
                
            }

            public void TranslateTransform(float dx, float dy, MatrixOrder order)
            {
                
            }

            public Region[] MeasureCharacterRanges(string text, Font font, RectangleF rectangle, StringFormat format)
            {
                return new Region[0];
            }

            public void DrawText(string text, float x, float y, Pen pen)
            {
                
            }

            public void Concat(Matrix matrix)
            {
                
            }

            public void FillBackground(Color color)
            {
                
            }
        }
    }
}
