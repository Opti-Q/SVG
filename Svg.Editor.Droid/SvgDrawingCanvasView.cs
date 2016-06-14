using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Svg.Core;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Droid.Editor.Services;
using Svg.Droid.Editor.Tools;
using Svg.Platform;
using Color = Android.Graphics.Color;
using GestureDetector = Svg.Droid.Editor.Tools.GestureDetector;
using InputEvent = Svg.Core.Events.InputEvent;

namespace Svg.Droid.Editor
{
    public class SvgDrawingCanvasView : ImageView
    {
        private SvgDrawingCanvas _drawingCanvas;
        private const int Size = 2000;

        public SvgDrawingCanvas DrawingCanvas => _drawingCanvas;
        public AndroidBitmap MainBitmap { get; } = new AndroidBitmap(Size, Size);

        public SvgDrawingCanvasView(Context context, IAttributeSet attr) : base(context, attr)
        {
            // Initialize SVG Platform and tie together PCL and platform specific modules
            SvgPlatformSetup.Init(new SvgAndroidPlatformOptions(context) {EnableFastTextRendering = true});

            _drawingCanvas = new SvgDrawingCanvas();
            //if (ZoomTool.IsActive)
            //    GestureDetector.Instance.ScaleDetector = new ScaleGestureDetector(context, new ZoomTool.ScaleListener(this, ViewModel.SelectionService));
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
            // Let the ScaleGestureDetector inspect all events.
            if(GestureDetector.Instance.ScaleDetector != null)
                GestureDetector.Instance.ScaleDetector.OnTouchEvent(ev); 

            DrawingCanvas.OnEvent(FromMotionEvent(ev));

            return true;
        }

        private InputEvent FromMotionEvent(MotionEvent ev)
        {
            // TODO LX: convert event information
            return new InputEvent();
        }

        protected override void OnDraw(Canvas canvas)
        {
            canvas.DrawColor(Color.White);

            //foreach (var tool in ViewModel.Tools.OrderBy(t => t.DrawOrder))
            //    tool.OnDraw(canvas, ViewModel.SelectionService.SelectedItem);

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


        //private void ResetTools()
        //{
        //    foreach (var tool in ViewModel.Tools.OrderBy(t => t.CommandOrder))
        //        tool.Reset();
        //}

        //public void ToggleGridVisibility()
        //{
        //    GridTool.IsVisible = !GridTool.IsVisible;
        //    Invalidate();
        //}

        //public void RemoveSelectedItem()
        //{
        //    ViewModel.Elements.Remove(ViewModel.SelectionService.SelectedItem);
        //    ViewModel.SelectionService.SelectedItem = null;
        //    ViewModel.SelectedItemChanged?.Invoke(false);
        //    Invalidate();
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DrawingCanvas?.Dispose();
                MainBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}