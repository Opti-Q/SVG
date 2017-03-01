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

            if (Element != null)
            {
                var oleElement = (SKFormsView) Element;
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

                    #region Register tools

                    // this part should be in the designer, when the iCL is created
                    var gridToolProperties = new Dictionary<string, object>
                    {
                        { "angle", 30.0f },
                        { "stepsizey", 20.0f },
                        { "issnappingenabled", true }
                    };

                    var zoomToolProperties = new Dictionary<string, object>();

                    var panToolProperties = new Dictionary<string, object>();

                    var undoRedoService = Engine.Resolve<IUndoRedoService>();

                    Engine.Register<ToolFactoryProvider, ToolFactoryProvider>(() => new ToolFactoryProvider(new Func<ITool>[]
                    {
                        () => new GridTool(gridToolProperties, undoRedoService),
                        () => new MoveTool(undoRedoService),
                        () => new PanTool(panToolProperties),
                        () => new ZoomTool(zoomToolProperties),
                        () => new SelectionTool(undoRedoService)
                    }));

                    #endregion

                    var drawingCanvas = new SvgDrawingCanvas();
                    Control.DrawingCanvas = drawingCanvas;
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
