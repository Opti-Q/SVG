using SkiaSharp.Views.Forms;

namespace Svg.Editor.Forms
{
    [RenderWith(typeof(SvgGlEditorViewRenderer))]
    public class SvgGlEditorView : SKCanvasView
    {
    }
}
