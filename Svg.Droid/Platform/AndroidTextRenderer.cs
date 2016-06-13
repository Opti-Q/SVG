using System.Linq;
using Android.Graphics;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidTextRenderer : IAlternativeSvgTextRenderer
    {
        public void Render(SvgTextBase txt, ISvgRenderer renderer)
        {
            if (!txt.Visible || !txt.Displayable)
                return;

            var brush = txt.Fill.GetBrush(txt, renderer, 1f);
            using (var pen = new AndroidPen(brush, txt.StrokeWidth.Value))
            {
                pen.TextSize = txt.FontSize.Value;
                pen.TextAlign = FromAnchor(txt.TextAnchor);

                var x = txt.X.Any() ? txt.X.FirstOrDefault().Value : 0f;
                var y = txt.Y.Any() ? txt.Y.FirstOrDefault().Value : 0f;
                renderer.DrawText(txt.Text, x, y, pen);
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