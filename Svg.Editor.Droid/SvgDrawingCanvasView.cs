using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using SkiaSharp;
using Svg.Droid.Editor.Services;
using Svg.Editor;
using Svg.Editor.Services;

namespace Svg.Droid.Editor
{
    public class SvgDrawingCanvasView : View
    {
#if !ANDROID
        private Android.Graphics.Bitmap _bitmap;
#endif
        private AndroidGestureDetector _detector;
        private SvgDrawingCanvas _drawingCanvas;

        public SvgDrawingCanvas DrawingCanvas
        {
            get { return _drawingCanvas; }
            set
            {
                _drawingCanvas = value;
                if (value == null) return;
                _detector?.Dispose();
                _detector = new AndroidGestureDetector(Context);
                _detector.DetectedGestures.Subscribe(async uie => await DrawingCanvas.OnEvent(uie));
            }
        }

        public SvgDrawingCanvasView(Context context, IAttributeSet attr) : base(context, attr)
        {
            //DrawingCanvas = new SvgDrawingCanvas();
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            // this is intentionally not awaited
            _detector.OnTouch(ev);
            
            return true;
        }

#if ANDROID
        protected override void OnDraw(Canvas canvas)
        {
            // this is intentionally not awaited
            DrawingCanvas.OnDraw(new AndroidCanvasRenderer(canvas))
                .ContinueWith(t => base.OnDraw(canvas));
        }

#else
        protected override async void OnDraw(Canvas canvas)
        {

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
#endif
        protected override void OnAttachedToWindow()
        {
            AndroidContextProvider._context = Context;

            base.OnAttachedToWindow();
            _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
            _drawingCanvas.CanvasInvalidated += OnCanvasInvalidated;
            _drawingCanvas.ToolCommandsChanged -= OnToolCommandsChanged;
            _drawingCanvas.ToolCommandsChanged += OnToolCommandsChanged;
        }

        protected override void OnDetachedFromWindow()
        {
            _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
            _drawingCanvas.ToolCommandsChanged -= OnToolCommandsChanged;
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