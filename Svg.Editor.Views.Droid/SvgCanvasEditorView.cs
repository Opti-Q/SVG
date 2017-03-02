using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using SkiaSharp;
using SkiaSharp.Views.Android;
using Svg.Editor.Droid.Services;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Shared;

namespace Svg.Editor.Views.Droid
{
    public class SvgCanvasEditorView : SKCanvasView, IPaintSurface
    {
        private Android.Graphics.Bitmap _bitmap;
        private AndroidGestureDetector _detector;
        private ISvgDrawingCanvas _drawingCanvas;
        private readonly Subject<UserInputEvent> _detectedGestures = new Subject<UserInputEvent>();

        public ISvgDrawingCanvas DrawingCanvas
        {
            get { return _drawingCanvas; }
            set
            {
                _drawingCanvas = value;
                if (value == null) return;
                _detector?.Dispose();
                _detector = new AndroidGestureDetector(Context);
                _detector.DetectedGestures.Subscribe(async uie => await DrawingCanvas.OnEvent(uie));
                _detector.DetectedGestures.Subscribe(_detectedGestures.OnNext);

                RegisterCallbacks();
            }
        }

        public SvgCanvasEditorView(Context context, IAttributeSet attr) : base(context, attr)
        {
            var gestureRecognizer = Engine.Resolve<IGestureRecognizer>() as GestureRecognizer;
            gestureRecognizer?.SubscribeTo(_detectedGestures.AsObservable());
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            // this is intentionally not awaited
            _detector.OnTouch(ev);
            
            return true;
        }

        protected override async void OnDraw(Canvas canvas)
        {
            if (DrawingCanvas == null)
                return;

            if (_bitmap == null || _bitmap.Width != canvas.Width || _bitmap.Height != canvas.Height)
            {
                _bitmap?.Dispose();

                _bitmap = Android.Graphics.Bitmap.CreateBitmap(canvas.Width, canvas.Height, Android.Graphics.Bitmap.Config.Argb8888);
            }

            try
            {
                using (var surface = SKSurface.Create(canvas.Width, canvas.Height, SKColorType.Rgba8888, SKAlphaType.Premul, _bitmap.LockPixels(), canvas.Width * 4))
                {
                    await DrawingCanvas.OnDraw(new SKCanvasRenderer(surface, canvas.Width, canvas.Height));
                }
            }
            finally
            {
                _bitmap.UnlockPixels();
            }

            canvas.DrawBitmap(_bitmap, 0, 0, null);

        }

        protected override void OnAttachedToWindow()
        {
            AndroidContextProvider._context = Context;

            base.OnAttachedToWindow();
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_drawingCanvas != null)
            {
                _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
                _drawingCanvas.CanvasInvalidated += OnCanvasInvalidated;
                _drawingCanvas.ToolCommandsChanged -= OnToolCommandsChanged;
                _drawingCanvas.ToolCommandsChanged += OnToolCommandsChanged;
            }
        }

        protected override void OnDetachedFromWindow()
        {
            if(_drawingCanvas != null)
            { 
                _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
                _drawingCanvas.ToolCommandsChanged -= OnToolCommandsChanged;
            }
            base.OnDetachedFromWindow();

            AndroidContextProvider._context = null;
        }

        private void OnCanvasInvalidated(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnToolCommandsChanged(object sender, EventArgs e)
        {
            ((Activity)Context).InvalidateOptionsMenu();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DrawingCanvas?.Dispose();
                _detector?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}