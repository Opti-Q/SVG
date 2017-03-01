using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Svg.Editor.Forms
{
    [RenderWith(typeof(SvgGlEditorViewRenderer))]
    public class SvgGlEditorView : SKCanvasViewX
    {
    }
}
