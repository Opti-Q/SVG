using System.Linq;
using Android.Graphics;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidTextRenderer : IAlternativeSvgTextRenderer
    {
        public void Render(SvgTextBase txt, ISvgRenderer renderer)
        {
            if (!txt.Visible || !txt.Displayable || string.IsNullOrEmpty(txt.Text))
                return;


            if(txt.Fill != null)
            {
                var brush = txt.Fill.GetBrush(txt, renderer, 1f);
                using (var pen = (AndroidPen)Engine.Factory.CreatePen(brush, 0f))
                {
                    pen.TextSize = txt.FontSize.Value;
                    pen.TextAlign = FromAnchor(txt.TextAnchor);

                    var x = txt.X.Any() ? txt.X.FirstOrDefault().Value : 0f;
                    var y = txt.Y.Any() ? txt.Y.FirstOrDefault().Value : 0f;
                    
                    pen.Style = Paint.Style.Fill;



                    DrawLines(txt, renderer, x, y, pen);
                }
            }

            if (txt.Stroke != null)
            {
                var brush = txt.Stroke.GetBrush(txt, renderer, 1f);
                using (var pen = (AndroidPen)Engine.Factory.CreatePen(brush, txt.StrokeWidth.Value))
                {
                    pen.TextSize = txt.FontSize.Value;
                    pen.TextAlign = FromAnchor(txt.TextAnchor);

                    var x = txt.X.Any() ? txt.X.FirstOrDefault().Value : 0f;
                    var y = txt.Y.Any() ? txt.Y.FirstOrDefault().Value : 0f;


                    pen.Style = Paint.Style.Stroke;
                    DrawLines(txt, renderer, x, y, pen);
                }
            }

        }

        private static void DrawLines(SvgTextBase txt, ISvgRenderer renderer, float x, float y, AndroidPen pen)
        {
            var lines = txt.Text.Split('\n');
            var b = txt.Bounds;
            var lineHeight = txt.Bounds.Height/lines.Length;
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                renderer.DrawText(lines[lineNumber], x, y + (lineHeight * lineNumber), pen);
            }
        }

        public RectangleF GetBounds(SvgTextBase txt, ISvgRenderer renderer)
        {
            if (!txt.Visible || !txt.Displayable || string.IsNullOrEmpty(txt.Text))
                return RectangleF.Create(0f, 0f, 0f, 0f);

            var brush = txt.Fill.GetBrush(txt, renderer, 1f);
            using (var pen = (AndroidPen)Engine.Factory.CreatePen(brush, txt.StrokeWidth.Value))
            {
                pen.TextSize = txt.FontSize.Value;
                pen.TextAlign = FromAnchor(txt.TextAnchor);

                var x = txt.X.Any() ? txt.X.FirstOrDefault().Value : 0f;
                var y = txt.Y.Any() ? txt.Y.FirstOrDefault().Value : 0f;


                float width = 0f;
                float height = 0f;

                Rect firstLineRect = null;
                var lines = txt.Text.Split('\n');
                var lineCount = lines.Length;

                // as android does not know the sense of "lines", we need to split the text and measure ourselves
                // see: http://stackoverflow.com/questions/6756975/draw-multi-line-text-to-canvas
                foreach (var line in lines)
                {
                    Rect rect = new Rect();
                    pen.Paint.GetTextBounds(line, 0, line.Length, rect);

                    if (firstLineRect == null)
                        firstLineRect = rect;

                    var w = rect.Right - rect.Left;
                    if (width < w)
                        width = w;

                    var h = rect.Bottom - rect.Top;
                    if (height < h)
                        height = h;
                }

                
                return RectangleF.Create(x + firstLineRect?.Left ?? 0f, y + firstLineRect?.Top ?? 0f, width, height * lineCount);
            }
        }

        private Paint.Align FromAnchor(SvgTextAnchor textAnchor)
        {
            switch (textAnchor)
            {
                case SvgTextAnchor.Middle:
                    return Paint.Align.Center;
                case SvgTextAnchor.End:
                    return Paint.Align.Right;
                case SvgTextAnchor.Start:
                    return Paint.Align.Left;
                default:
                    return Paint.Align.Left;
            }
        }
    }
}