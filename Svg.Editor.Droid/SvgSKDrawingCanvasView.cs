using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using SkiaSharp;
using Svg.Core;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Droid.Editor.Services;
using GestureDetector = Svg.Droid.Editor.Services.GestureDetector;

namespace Svg.Droid.Editor
{
    public class SvgSKDrawingCanvasView : View
    {
        private readonly GestureDetector _detector;
        private SvgDrawingCanvas _drawingCanvas;

        public SvgDrawingCanvas DrawingCanvas
        {
            get { return _drawingCanvas; }
            set { _drawingCanvas = value; }
        }

        public SvgSKDrawingCanvasView(Context context, IAttributeSet attr) : base(context, attr)
        {
            // Initialize SVG Platform and tie together PCL and platform specific modules
            SvgPlatformSetup.Init(new SvgAndroidPlatformOptions() {EnableFastTextRendering = true});
            Engine.Register<IRendererFactory, AndroidCanvasRendererFactory>(() => new AndroidCanvasRendererFactory());
            Engine.Register<IImageStorer, IImageStorer>(() => new ImageStorer());

            _drawingCanvas = new SvgDrawingCanvas();
            _detector = new GestureDetector(this.Context, (e) => DrawingCanvas.OnEvent(e));

            Engine.Register<ITextInputService, TextInputService>(() => new TextInputService(context));
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            // this is intentionally not awaited
            _detector.OnTouch(ev).ConfigureAwait(false);
            
            return true;
        }

        protected override void OnDraw(Canvas canvas)
        {
            ////// this is intentionally not awaited
            ////DrawingCanvas.OnDraw(new AndroidCanvasRenderer(canvas))
            ////    .ContinueWith(t => base.OnDraw(canvas));
            //var width = (float)skiaView.Width;
            //var height = (float)skiaView.Height;

            ///*using (*/
            //var bitmap = Android.Graphics.Bitmap.CreateBitmap(canvas.Width, canvas.Height,
            //    Android.Graphics.Bitmap.Config.Argb8888);/*)*/
            //{
            //    //try
            //    //{
            //        /*using (*/var surface = SKSurface.Create(canvas.Width, canvas.Height, SKColorType.Rgba_8888, SKAlphaType.Premul, bitmap.LockPixels(), canvas.Width * 4);/*)*/
            //        {
            //            var skcanvas = surface.Canvas;
            //            skcanvas.Scale(((float)canvas.Width) / width, ((float)canvas.Height) / height);

            //            DrawingCanvas.OnDraw(new SKCanvasRenderer(surface))
            //                .ContinueWith(t =>
            //                {
            //                    using (surface)
            //                    using (bitmap)
            //                    {
            //                        bitmap.UnlockPixels();
            //                        canvas.DrawBitmap(bitmap, 0, 0, null);
            //                    }
            //                });
            //        }
            //    //}
            //    //finally
            //    //{
            //    //    bitmap.UnlockPixels();
            //    //}
            //    //canvas.DrawBitmap(bitmap, 0, 0, null);
            //}
        }

        protected override void OnAttachedToWindow()
        {
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
        }

        private void OnCanvasInvalidated(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnToolCommandsChanged(object sender, EventArgs e)
        {
            ((Activity)this.Context).InvalidateOptionsMenu();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DrawingCanvas?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}