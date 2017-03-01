using Windows.UI.Xaml.Controls;
using Svg.Editor.Forms;
using Svg.Editor.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(SvgCanvasEditorViewRenderer), typeof(SvgCanvasEditorView))]
namespace Svg.Editor.Forms
{
    internal class SvgCanvasEditorViewRenderer : ViewRenderer<SvgCanvasEditorView, Canvas>
    {
        private UwpGestureRecognizer _gestureRecognizer;

        //protected override void OnElementChanged(ElementChangedEventArgs<SKCanvasView> e)
        //{
        //    base.OnElementChanged(e);

        //    _gestureRecognizer = new UwpGestureRecognizer(Control);
        //}

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
