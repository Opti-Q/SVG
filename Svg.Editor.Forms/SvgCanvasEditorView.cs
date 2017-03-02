using System;
using SkiaSharp.Views.Forms;

namespace Svg.Editor.Forms
{
    public class SvgCanvasEditorView : SKCanvasViewX
    {
        protected override void OnPropertyChanging(string propertyName = null)
        {
            if (propertyName == "BindingContext")
            {
                var canvas = BindingContext as SvgDrawingCanvas;
                if (canvas != null)
                {
                    canvas.CanvasInvalidated -= OnCanvasInvalidated;
                }
            }

            base.OnPropertyChanging(propertyName);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == "BindingContext")
            {

                var canvas = BindingContext as SvgDrawingCanvas;
                if (canvas != null)
                {
                    canvas.CanvasInvalidated += OnCanvasInvalidated;
                }

            }

            base.OnPropertyChanged(propertyName);
        }

        private void OnCanvasInvalidated(object sender, EventArgs e)
        {
            InvalidateSurface();
        }
    }
}
