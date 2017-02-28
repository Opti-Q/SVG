using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Svg.Editor.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly:ExportRenderer(typeof(SvgCanvasEditorRenderer), typeof(SvgCanvasEditorView))]
namespace Svg.Editor.UWP
{
    public class SvgCanvasEditorRenderer : SKCanvasViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SKCanvasView> e)
        {
            base.OnElementChanged(e);
        }
    }
}
