using SkiaSharp.Views.Forms;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;

namespace Svg.Editor.Forms
{
    public class SvgCanvasEditorView : SKCanvasViewX
    {
        private ISvgDrawingCanvas _drawingCanvas;

        public ISvgDrawingCanvas DrawingCanvas
        {
            get { return _drawingCanvas; }
            set
            {
                _drawingCanvas = value;
                if (value == null) return;
                RegisterCallbacks();
            }
        }

        private void RegisterCallbacks()
        {
            DrawingCanvas.CanvasInvalidated += DrawingCanvas_CanvasInvalidated;
            DrawingCanvas.ToolCommandsChanged += DrawingCanvas_ToolCommandsChanged;
        }

        private void DrawingCanvas_ToolCommandsChanged(object sender, System.EventArgs e)
        {
        }

        private void DrawingCanvas_CanvasInvalidated(object sender, System.EventArgs e)
        {
            InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            DrawingCanvas?.OnDraw(new SKCanvasRenderer(e.Surface, e.Info.Width, e.Info.Height));
        }
    }
}
