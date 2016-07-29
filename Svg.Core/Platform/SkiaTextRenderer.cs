using System.Linq;
using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaTextRenderer : IAlternativeSvgTextRenderer
    {
        public void Render(SvgTextBase txt, ISvgRenderer renderer)
        {
            if (!txt.Visible || !txt.Displayable)
                return;

            bool textIsEmpty = string.IsNullOrEmpty(txt.Text);
            bool hasNoChildren = !txt.Children.OfType<SvgTextBase>().Any();

            if (textIsEmpty && hasNoChildren)
                return;


            if (!textIsEmpty)
            {
                RenderFill(txt, renderer);
                RenderStroke(txt, renderer);
            }

            if (!hasNoChildren)
            {
                // render children (text spans and the like)
                foreach (var child in txt.Children.OfType<SvgTextBase>())
                    Render(child, renderer);
            }
        }

        private void RenderStroke(SvgTextBase txt, ISvgRenderer renderer)
        {
            if (txt.Stroke != null)
            {
                var brush = txt.Stroke.GetBrush(txt, renderer, 1f);
                using (var pen = (SkiaPen)Engine.Factory.CreatePen(brush, txt.StrokeWidth.Value))
                {
                    pen.TextSize = txt.FontSize.Value;
                    pen.TextAlign = FromAnchor(txt.TextAnchor);

                    var x = txt.X.Any() ? txt.X.FirstOrDefault().Value : 0f;
                    var y = txt.Y.Any() ? txt.Y.FirstOrDefault().Value : 0f;


                    pen.Paint.IsStroke = true;
                    DrawLines(txt, renderer, x, y, pen);
                }
            }
        }

        private void RenderFill(SvgTextBase txt, ISvgRenderer renderer)
        {
            if (txt.Fill != null)
            {
                var brush = txt.Fill.GetBrush(txt, renderer, 1f);
                using (var pen = (SkiaPen)Engine.Factory.CreatePen(brush, 0f))
                {
                    pen.TextSize = txt.FontSize.Value;
                    pen.TextAlign = FromAnchor(txt.TextAnchor);

                    var x = txt.X.Any() ? txt.X.FirstOrDefault().Value : 0f;
                    var y = txt.Y.Any() ? txt.Y.FirstOrDefault().Value : 0f;

                    pen.Paint.IsStroke = false;

                    DrawLines(txt, renderer, x, y, pen);
                }
            }
        }

        private static void DrawLines(SvgTextBase txt, ISvgRenderer renderer, float x, float y, SkiaPen pen)
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
            if (!txt.Visible || !txt.Displayable)
                return RectangleF.Create(0f, 0f, 0f, 0f);

            bool textIsEmpty = string.IsNullOrEmpty(txt.Text);
            bool hasNoChildren = !txt.Children.OfType<SvgTextBase>().Any();

            if (textIsEmpty && hasNoChildren)
                return RectangleF.Create(0f, 0f, 0f, 0f);

            if (!textIsEmpty)
            {
                var brush = txt.Fill.GetBrush(txt, renderer, 1f);
                using (var pen = (SkiaPen) Engine.Factory.CreatePen(brush, txt.StrokeWidth.Value))
                {
                    pen.TextSize = txt.FontSize.Value;
                    pen.TextAlign = FromAnchor(txt.TextAnchor);

                    var x = txt.X.Any() ? txt.X.FirstOrDefault().Value : 0f;
                    var y = txt.Y.Any() ? txt.Y.FirstOrDefault().Value : 0f;

                    float width = 0f;
                    float height = 0f;

                    SKRect firstLineRect = default(SKRect);
                    var lines = txt.Text.Split('\n');
                    var lineCount = lines.Length;

                    // as android does not know the sense of "lines", we need to split the text and measure ourselves
                    // see: http://stackoverflow.com/questions/6756975/draw-multi-line-text-to-canvas
                    foreach (var line in lines)
                    {
                        SKRect rect = new SKRect();
                        pen.Paint.MeasureText(line, ref rect);

                        if (IsEmpty(rect))
                            firstLineRect = rect;

                        var w = rect.Right - rect.Left;
                        if (width < w)
                            width = w;

                        var h = rect.Bottom - rect.Top;
                        if (height < h)
                            height = h;
                    }


                    return RectangleF.Create(x + firstLineRect.Left, y + firstLineRect.Top, width,
                        height*lineCount);
                }
            }
            else if(!hasNoChildren)
            {
                RectangleF r = null;

                foreach (var child in txt.Children.OfType<SvgTextBase>())
                {
                    var bounds = GetBounds(child, renderer);
                    if (r == null)
                        r = bounds;
                    else
                    {
                        r = r.UnionAndCopy(bounds);
                    }
                }

                return r;
            }

            return RectangleF.Create(0f, 0f, 0f, 0f);
        }

        private bool IsEmpty(SKRect rect)
        {
            return rect.Top == 0f && rect.Left == 0f && rect.Bottom == 0f && rect.Right == 0f;
        }

        private SKTextAlign FromAnchor(SvgTextAnchor textAnchor)
        {
            switch (textAnchor)
            {
                case SvgTextAnchor.Middle:
                    return SKTextAlign.Center;
                case SvgTextAnchor.End:
                    return SKTextAlign.Right;
                case SvgTextAnchor.Start:
                    return SKTextAlign.Left;
                default:
                    return SKTextAlign.Left;
            }
        }
    }
}