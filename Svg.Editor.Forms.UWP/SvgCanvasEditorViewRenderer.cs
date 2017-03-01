using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Svg.Editor.Views;
using Xamarin.Forms.Platform.UWP;

using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;
using SKNativeView = SkiaSharp.Views.UWP.SKXamlCanvasX;

[assembly: ExportRenderer(typeof(SvgCanvasEditorView), typeof(SvgCanvasEditorViewRenderer))]
namespace Svg.Editor.Forms
{
    public class SvgCanvasEditorViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
    {
        private UwpGestureRecognizer _gestureRecognizer;

        protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
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
