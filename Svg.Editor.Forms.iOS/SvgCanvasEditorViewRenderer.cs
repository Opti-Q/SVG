using System;
using System.ComponentModel;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;
using SKNativeView = Svg.Editor.iOS.SvgCanvasEditorView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SvgCanvasEditorViewRenderer))]
namespace Svg.Editor.Forms
{
    public class SvgCanvasEditorViewRenderer : ViewRenderer<SKFormsView, SKNativeView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
        {
            if (e.OldElement != null)
            {
                var oldController = (ISKCanvasViewController)e.OldElement;

                // unsubscribe from events
                oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
                oldController.GetCanvasSize -= OnGetCanvasSize;
            }

            if (e.NewElement != null)
            {
                var newController = (ISKCanvasViewController)e.NewElement;

                // create the native view
                var view = new InternalView(newController);
                view.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;
                SetNativeControl(view);

                // subscribe to events from the user
                newController.SurfaceInvalidated += OnSurfaceInvalidated;
                newController.GetCanvasSize += OnGetCanvasSize;

                // paint for the first time
                Control.SetNeedsDisplay();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(SKFormsView.IgnorePixelScaling))
            {
                Control.IgnorePixelScaling = Element.IgnorePixelScaling;
            }
        }

        protected override void Dispose(bool disposing)
        {
            // detach all events before disposing
            var controller = (ISKCanvasViewController)Element;
            if (controller != null)
            {
                controller.SurfaceInvalidated -= OnSurfaceInvalidated;
            }

            base.Dispose(disposing);
        }

        // the user asked for the size
        private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
        {
            e.CanvasSize = Control?.CanvasSize ?? SKSize.Empty;
        }

        private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
        {
            // repaint the native control
            Control.SetNeedsDisplay();
        }

        private class InternalView : SKNativeView
        {
            private readonly ISKCanvasViewController controller;

            public InternalView(ISKCanvasViewController controller)
            {
                UserInteractionEnabled = false;

                this.controller = controller;

                // Force the opacity to false for consistency with the other platforms
                Opaque = false;
            }

            public override void DrawInSurface(SKSurface surface, SKImageInfo info)
            {
                base.DrawInSurface(surface, info);

                // the control is being repainted, let the user know
                controller.OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
            }
        }
    }
}
