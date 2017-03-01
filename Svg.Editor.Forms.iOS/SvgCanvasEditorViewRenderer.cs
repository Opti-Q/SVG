using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Xamarin.Forms;
using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;
using SKNativeView = Svg.Editor.iOS.SvgCanvasEditorView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SvgCanvaseEditorViewRenderer))]

namespace Svg.Editor.Forms
{
    public class SvgCanvaseEditorViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
    {
    }
}
