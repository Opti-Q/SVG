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
        private UwpGestureDetector _gestureDetector;
        private IDisposable _pressedToken;
        private IDisposable _releasedToken;
        private IDisposable _movedToken;


        protected override void OnElementChanged(ElementChangedEventArgs<SKCanvasView> e)
        {
            base.OnElementChanged(e);

            _gestureDetector = new UwpGestureDetector(Control);

            _pressedToken = Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>
            (
                h => (_, args) => h(args),
                h => Control.PointerPressed += h,
                h => Control.PointerPressed -= h
            ).Subscribe(args => _gestureDetector.OnTouch(args));

            _releasedToken = Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>
            (
                h => (_, args) => h(args),
                h => Control.PointerReleased += h,
                h => Control.PointerReleased -= h
            ).Subscribe(args => _gestureDetector.OnTouch(args));

            _movedToken = Observable.FromEvent<PointerEventHandler, PointerRoutedEventArgs>
            (
                h => (_, args) => h(args),
                h => Control.PointerMoved += h,
                h => Control.PointerMoved -= h
            ).Subscribe(args => _gestureDetector.OnTouch(args));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _pressedToken.Dispose();
            _releasedToken.Dispose();
            _movedToken.Dispose();
        }
    }
}
