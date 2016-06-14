using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Svg.Core;
using Svg.Droid.Editor.Services;
using Svg.Platform;
using Color = Android.Graphics.Color;
using GestureDetector = Svg.Droid.Editor.Tools.GestureDetector;

namespace Svg.Droid.Editor
{
    public class SvgDrawingCanvasView : ImageView
    {
        private const int Size = 2000;
        private readonly SvgDrawingCanvas _drawingCanvas;
        private readonly GestureDetector _detector;

        public SvgDrawingCanvas DrawingCanvas => _drawingCanvas;
        public AndroidBitmap Bitmap { get; } = new AndroidBitmap(Size, Size);

        public SvgDrawingCanvasView(Context context, IAttributeSet attr) : base(context, attr)
        {
            // Initialize SVG Platform and tie together PCL and platform specific modules
            SvgPlatformSetup.Init(new SvgAndroidPlatformOptions(context) {EnableFastTextRendering = true});

            _drawingCanvas = new SvgDrawingCanvas();
            _detector = new GestureDetector(this.Context, (e) => DrawingCanvas.OnEvent(e));
        }

        public void AddSvg(SvgDocument svgDocument)
        {
            //// TODO PUT ME IN THE VIEWMODEL

            //ViewModel.AddSvg(svgDocument);
            //var bitmap = (AndroidBitmap) svgDocument.Draw();
            //var x = (Width / 2) - (bitmap.Width / 2) - (int) SharedMasterTool.Instance.CanvasTranslatedPosX;
            //var y = (Height / 2) - (bitmap.Height / 2) - (int) SharedMasterTool.Instance.CanvasTranslatedPosY;

            //if (SnappingTool.IsActive)
            //{
            //    x = (int) (Math.Round((x) / SnappingTool.StepSize) * SnappingTool.StepSize);
            //    y = (int) (Math.Round((y) / SnappingTool.StepSize) * SnappingTool.StepSize);
            //}

            //var selBitmap = new SelectableAndroidBitmap(bitmap, x, y);

            //ViewModel.Elements.Add(selBitmap);
            ////ViewModel.Select(selBitmap);

            //ViewModel.Select(ViewModel.Elements.LastOrDefault());
            //Invalidate();
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            _detector.OnTouch(ev);
            
            return true;
        }

        protected override void OnDraw(Canvas canvas)
        {
            canvas.DrawColor(Color.White);

            //foreach (var bitmap in ViewModel.Elements)
            //    canvas.DrawBitmap(bitmap.Image, bitmap.X, bitmap.Y, null);
            DrawingCanvas.OnDraw(new AndroidCanvasRenderer(canvas));

            base.OnDraw(canvas);
        }
        
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
            _drawingCanvas.CanvasInvalidated += OnCanvasInvalidated;
        }

        private void OnCanvasInvalidated(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnDetachedFromWindow()
        {
            _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
            base.OnDetachedFromWindow();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DrawingCanvas?.Dispose();
                Bitmap?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}