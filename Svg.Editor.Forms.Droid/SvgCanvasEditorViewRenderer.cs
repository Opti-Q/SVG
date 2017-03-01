using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Xamarin.Forms;
using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;
using SKNativeView = Svg.Editor.Views.Droid.SvgCanvasEditorView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SvgCanvaseEditorViewRenderer))]

namespace Svg.Editor.Forms
{
    public class SvgCanvaseEditorViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
    {
    }
}
