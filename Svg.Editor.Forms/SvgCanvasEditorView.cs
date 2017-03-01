using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Svg.Editor.Forms
{
    public class SvgCanvasEditorView : SKCanvasView
    {

    }
    public interface ISKCanvasViewController : IViewController
    {
        // the native listens to this event
        event EventHandler SurfaceInvalidated;
        event EventHandler<GetCanvasSizeEventArgs> GetCanvasSize;

        // the native view tells the user to repaint
        void OnPaintSurface(SKPaintSurfaceEventArgs e);
    }
    public class GetCanvasSizeEventArgs : EventArgs
    {
        public SKSize CanvasSize { get; set; }
    }
}
