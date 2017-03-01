using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Svg.Editor.Forms.UWP;
using Svg.Editor.Views.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(SvgCanvasEditorView), typeof(SvgCanvasEditorViewRenderer))]
namespace Svg.Editor.Forms.UWP
{
    public class SvgCanvasEditorViewRenderer : SKCanvasViewRendererBase<SvgCanvasEditorView, SkiaSharp.Views.UWP.SKXamlCanvasX>
    {
        private UwpGestureRecognizer _gestureRecognizer;

        protected override void OnElementChanged(ElementChangedEventArgs<SvgCanvasEditorView> e)
        {
            base.OnElementChanged(e);

            _gestureRecognizer = new UwpGestureRecognizer(Control);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _gestureRecognizer.Dispose();
        }
    }
}
