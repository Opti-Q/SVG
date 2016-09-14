using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Xml;
using Android.Graphics;
using Svg.Interfaces;
using Svg.Interfaces.Xml;
using Svg.Platform;
using Color = Svg.Interfaces.Color;
using PointF = Svg.Interfaces.PointF;
using RectangleF = Svg.Interfaces.RectangleF;

namespace Svg
{
    public class Factory : IFactory
    {
        public static IFactory Instance = new Factory();

        private AndroidColors _colors = new AndroidColors();

        public GraphicsPath CreateGraphicsPath()
        {
            return new AndroidGraphicsPath();
        }

        public GraphicsPath CreateGraphicsPath(FillMode fillmode)
        {
            return new AndroidGraphicsPath(fillmode);
        }

        public Region CreateRegion(RectangleF rect)
        {
            return new Region(rect);
        }
        
        public Pen CreatePen(Brush brush, float strokeWidth)
        {
            return new AndroidPen(brush, strokeWidth);
        }

        public Matrix CreateMatrix()
        {
            return new AndroidMatrix();
        }

        public Matrix CreateIdentityMatrix()
        {
            var m = new Android.Graphics.Matrix();
            m.Reset();
            return new AndroidMatrix(m);
        }

        public Matrix CreateMatrix(float i, float i1, float i2, float i3, float i4, float i5)
        {
            return new AndroidMatrix(i, i1, i2, i3, i4, i5);
        }

        public Bitmap CreateBitmap(Image inputImage)
        {
            return new AndroidBitmap(inputImage);
        }

        public Bitmap CreateBitmap(int width, int height)
        {
            return new AndroidBitmap(width, height);
        }

        public Graphics CreateGraphicsFromImage(Bitmap input)
        {
            var bitmap = (AndroidBitmap)input;
            return new AndroidGraphics(bitmap);
        }

        public Graphics CreateGraphicsFromImage(Image image)
        {
            var bitmap = (AndroidBitmap) image;
            return new AndroidGraphics(bitmap);
        }

        public ColorMatrix CreateColorMatrix(float[][] colorMatrixElements)
        {
            return new AndroidColorMatrix(colorMatrixElements);
        }

        public ImageAttributes CreateImageAttributes()
        {
            throw new System.NotImplementedException();
        }

        public SolidBrush CreateSolidBrush(Color color)
        {
            return new AndroidSolidBrush((AndroidColor)color);
        }

        public ColorBlend CreateColorBlend(int colourBlends)
        {
            return new ColorBlend(colourBlends);
        }

        public TextureBrush CreateTextureBrush(Bitmap image)
        {
            return new AndroidTextureBrush((AndroidBitmap) image);
        }

        public LinearGradientBrush CreateLinearGradientBrush(PointF start, PointF end, Color startColor, Color endColor)
        {
            return new AndroidLinearGradientBrush((AndroidPointF)start, (AndroidPointF)end, (AndroidColor)startColor, (AndroidColor)endColor);
        }

        public PathGradientBrush CreatePathGradientBrush(GraphicsPath path)
        {
            throw new System.NotImplementedException();
        }

        public StringFormat CreateStringFormatGenericTypographic()
        {
            return new AndroidStringFormat();
        }

        public Font CreateFont(FontFamily fontFamily, float fontSize, FontStyle fontStyle, GraphicsUnit graphicsUnit)
        {
            var font = new AndroidFont((AndroidFontFamily) fontFamily);
            font.Size = fontSize;
            font.Style = fontStyle;
            // TODO LX: what to use graphicsUnit for?

            return font;
        }

        public FontFamilyProvider GetFontFamilyProvider()
        {
            return new AndroidFontFamilyProvider();
        }

        public Image CreateImageFromStream(Stream stream)
        {
            var bitmap = BitmapFactory.DecodeStream(stream);
            return new AndroidBitmap(bitmap);
        }

        public Bitmap CreateBitmapFromStream(Stream stream)
        {
            var bitmap = BitmapFactory.DecodeStream(stream);
            return new AndroidBitmap(bitmap);
        }

        public RectangleF CreateRectangleF(PointF location, SizeF size)
        {
            return new AndroidRectangleF(location.X, location.Y, size.Width, size.Height);
        }

        public RectangleF CreateRectangleF(float left, float top, float width, float height)
        {
            return new AndroidRectangleF(left, top, width, height);
        }

        public RectangleF CreateRectangleF()
        {
            return new AndroidRectangleF(0, 0, 0, 0);
        }

        public Colors Colors => _colors;

        public Color CreateColorFromArgb(int alpha, Color color)
        {
            return new AndroidColor((byte)alpha, color);
        }

        public Color CreateColorFromArgb(int alpha, int r, int g, int b)
        {
            return new AndroidColor((byte)alpha, (byte)r, (byte)g, (byte)b);
        }

        public Color CreateColorFromHexString(string hex)
        {
            throw new NotImplementedException();
        }

        public PointF CreatePointF(float x, float y)
        {
            return new AndroidPointF(x, y);
        }

        public SizeF CreateSizeF(float width, float height)
        {
            return new AndroidSizeF(width, height);
        }

        public IXmlTextWriter CreateXmlTextWriter(StringWriter writer)
        {
            return new SvgXmlTextWriter(writer);
        }

        public IXmlTextWriter CreateXmlTextWriter(Stream stream, Encoding utf8)
        {
            var w = new SvgXmlTextWriter(stream, utf8);
            w.Formatting = Formatting.Indented;
            return w;
        }
        public XmlReader CreateSvgTextReader(Stream stream, Dictionary<string, string> entities)
        {
            var reader = new SvgTextReader(stream, entities);
            reader.XmlResolver = new SvgDtdResolver();
            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            return reader;
        }

        public XmlReader CreateSvgTextReader(StringReader r, Dictionary<string, string> entities)
        {
            var reader = new SvgTextReader(r, entities);
            reader.XmlResolver = new SvgDtdResolver();
            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            return reader;
        }

        public ISortedList<TKey, TValue> CreateSortedList<TKey, TValue>()
        {
            return new SvgSortedList<TKey, TValue>();
        }
        public IDictionary<TKey, TValue> CreateSortedDictionary<TKey, TValue>()
        {
            return new SortedDictionary<TKey, TValue>();
        }
    }
}