using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;
using SKNativeView = Svg.Editor.Droid.SvgCanvasEditorView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SvgCanvaseEditorViewRenderer))]

namespace Svg.Editor.Forms
{
    public class SvgCanvaseEditorViewRenderer : SKCanvasViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SKCanvasView> e)
        {
            if (Control != null)
            {
                // clean up old native control
            }

            if (e.OldElement != null)
            {
                var oldTouchCanvas = (SKFormsView)e.OldElement;

                // do clean up old element
            }

            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                var newTouchCanvas = (SKFormsView)Element;

                // set up new element
            }
        }
    }
}
