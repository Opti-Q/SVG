using System;
using System.Reactive.Linq;
using Windows.UI.Xaml.Input;
using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Svg.Editor.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(SvgCanvasEditorRenderer), typeof(SvgCanvasEditorView))]
namespace Svg.Editor.UWP
{
    public class SvgCanvasEditorRenderer : SKCanvasViewRenderer
    {
        private readonly UwpGestureDetector _gestureDetector = new UwpGestureDetector();
        private IDisposable _pressedToken;


        protected override void OnElementChanged(ElementChangedEventArgs<SKCanvasView> e)
        {
            base.OnElementChanged(e);

            _pressedToken = Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>
            (
                h => (_, args) => h(args),
                h => Control.PointerPressed += h,
                h => Control.PointerPressed -= h
            ).Subscribe(args => _gestureDetector.OnTouch(args));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _pressedToken.Dispose();
        }
    }
}
