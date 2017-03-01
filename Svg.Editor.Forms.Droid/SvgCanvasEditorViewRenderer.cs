using System;
using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;
using SKNativeView = Svg.Editor.Droid.SvgCanvasEditorView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SvgCanvasEditorViewRenderer))]

namespace Svg.Editor.Forms
{
    public class SvgCanvasEditorViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SvgCanvasEditorView> e)
        {
            if (Control != null)
            {
                Control.DrawingCanvas = null;
            }

            if (Element != null)
            {
                var oleElement = (SKFormsView)Element;
                oleElement.BindingContextChanged -= OnElementBindingContextChanged;

                // do clean up old element
            }

            base.OnElementChanged(e);


            if (e.NewElement != null)
            {
                var newElement = e.NewElement;
                newElement.BindingContextChanged += OnElementBindingContextChanged;
                if (Control != null)
                {
                    Control.DrawingCanvas = newElement.BindingContext as SvgDrawingCanvas;
                }
            }
        }

        private void OnElementBindingContextChanged(object sender, EventArgs e)
        {
            var fwe = sender as BindableObject;

            if (fwe != null && Control != null)
            {
                Control.DrawingCanvas = fwe.BindingContext as SvgDrawingCanvas;
            }
        }
    }
}
