using System;
using System.Collections.Generic;
using SkiaSharp.Views.Forms;
using Svg.Editor.Forms.Droid;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;
using SKNativeView = Svg.Editor.Views.Droid.SvgCanvasEditorView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SvgCanvasEditorViewRenderer))]
namespace Svg.Editor.Forms.Droid
{
    public class SvgCanvasEditorViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
        {
            if (Control != null)
            {
                Control.DrawingCanvas = null;
            }

            //if (Element != null)
            //{
            //    var oleElement = (SKFormsView) Element;

            //    // do clean up old element
            //}

            base.OnElementChanged(e);


            if (e.NewElement != null)
            {
                var newElement = e.NewElement;
                UpdateBindings(newElement);
            }
        }

        protected override SKNativeView CreateNativeView()
        {
            var nv = base.CreateNativeView();
            nv.IsFormsMode = true;
            return nv;
        }

        private void OnElementBindingContextChanged(object sender, EventArgs e)
        {
            UpdateBindings(sender as BindableObject);
        }

        private void UpdateBindings(BindableObject fwe)
        {
            if (fwe != null)
            {
                if (Control != null)
                {
                    Control.DrawingCanvas = fwe.BindingContext as SvgDrawingCanvas;
                }
            }
        }
    }
}
