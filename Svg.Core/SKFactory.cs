using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Xml;
using SkiaSharp;
using Svg.Interfaces;
using Svg.Interfaces.Xml;
using Svg.Platform;

namespace Svg
{
    public abstract class SKFactoryBase : IFactory
    {
        private static readonly Colors _colors = new SkiaColors();

        public GraphicsPath CreateGraphicsPath()
        {
            return new SkiaGraphicsPath();
        }

        public GraphicsPath CreateGraphicsPath(FillMode winding)
        {
            return new SkiaGraphicsPath(winding);
        }

        public Region CreateRegion(RectangleF rect)
        {
            return new Region(rect);
        }

        public Pen CreatePen(Brush brush, float strokeWidth)
        {
            return new SkiaPen(brush, strokeWidth);
        }

        public Matrix CreateMatrix()
        {
            return new SkiaMatrix();
        }

        public Matrix CreateMatrix(float scaleX, float rotateX, float rotateY, float scaleY, float transX, float transY)
        {
            return new SkiaMatrix(scaleX, rotateX, rotateY, scaleY, transX, transY);
        }

        public Bitmap CreateBitmap(Image inputImage)
        {
            return new SkiaBitmap(inputImage);
        }

        public Bitmap CreateBitmap(int width, int height)
        {
            return new SkiaBitmap(width, height);
        }

        public Graphics CreateGraphicsFromImage(Bitmap input)
        {
            return new SkiaGraphics((SkiaBitmap)input);
        }

        public Graphics CreateGraphicsFromImage(Image image)
        {
            return new SkiaGraphics((SkiaBitmap)image);
        }

        public ColorMatrix CreateColorMatrix(float[][] colorMatrixElements)
        {
            return new SkiaColorMatrix(colorMatrixElements);
        }

        public ImageAttributes CreateImageAttributes()
        {
            throw new NotImplementedException();
        }

        public SolidBrush CreateSolidBrush(Color color)
        {
            return new SkiaSolidBrush(color);
        }

        public ColorBlend CreateColorBlend(int colourBlends)
        {
            return new ColorBlend(colourBlends);
        }

        public TextureBrush CreateTextureBrush(Bitmap image)
        {
            return new SkiaTextureBrush((SkiaBitmap)image);
        }

        public LinearGradientBrush CreateLinearGradientBrush(PointF start, PointF end, Color startColor, Color endColor)
        {
            return new SkiaLinearGradientBrush(start, end, startColor, endColor);
        }

        public PathGradientBrush CreatePathGradientBrush(GraphicsPath path)
        {
            throw new NotImplementedException();
        }

        public StringFormat CreateStringFormatGenericTypographic()
        {
            return new SkiaStringFormat();
        }

        public Font CreateFont(FontFamily fontFamily, float fontSize, FontStyle fontStyle, GraphicsUnit graphicsUnit)
        {
            var font = new SkiaFont((SkiaFontFamily)fontFamily);
            font.Size = fontSize;
            font.Style = fontStyle;
            // TODO LX: what to use graphicsUnit for?

            return font;
        }

        public FontFamilyProvider GetFontFamilyProvider()
        {
            return new SkiaFontFamilyProvider();
        }

        public RectangleF CreateRectangleF()
        {
            return new SkiaRectangleF();
        }

        public RectangleF CreateRectangleF(PointF location, SizeF size)
        {
            return new SkiaRectangleF(location, size);
        }

        public RectangleF CreateRectangleF(float left, float top, float width, float height)
        {
            return new SkiaRectangleF(left, top, width, height);
        }

        public Colors Colors => _colors;
        public Color CreateColorFromArgb(int alpha, Color colour)
        {
            return new SkiaColor((byte)alpha, colour);
        }

        public Color CreateColorFromArgb(int alpha, int r, int g, int b)
        {
            return new SkiaColor((byte) alpha, (byte)r, (byte)g, (byte)b);
        }

        public PointF CreatePointF(float x, float y)
        {
            return new SkiaPointF(x, y);
        }

        public SizeF CreateSizeF(float width, float height)
        {
            return new SkiaSizeF(width, height);
        }

        public IDictionary<TKey, TValue> CreateSortedDictionary<TKey, TValue>()
        {
            return new SortedDictionary<TKey, TValue>();
        }

        public Bitmap CreateBitmapFromStream(Stream stream)
        {
            using (var ms = new MemoryStream())
            using (var s = new SKMemoryStream())
            {
                s.SetMemory(ms.ToArray());
                var bm = SKBitmap.Decode(s);
                return new SkiaBitmap(bm);
            }
        }

        public Image CreateImageFromStream(Stream stream)
        {
            using(var ms = new MemoryStream())
            using (var s = new SKMemoryStream())
            {
                s.SetMemory(ms.ToArray());

                var bm = SKBitmap.Decode(s);
                return new SkiaBitmap(bm);
            }
        }

        public abstract ISortedList<TKey, TValue> CreateSortedList<TKey, TValue>();

        public abstract IXmlTextWriter CreateXmlTextWriter(StringWriter writer);

        public abstract IXmlTextWriter CreateXmlTextWriter(Stream stream, Encoding utf8);
        public abstract XmlReader CreateSvgTextReader(Stream stream, Dictionary<string, string> entities);

        public abstract XmlReader CreateSvgTextReader(StringReader r, Dictionary<string, string> entities);
    }
}