using System;
using System.Collections.Generic;
using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Svg.Editor.Forms.UWP;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Editor.Views.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(SvgCanvasEditorView), typeof(SvgCanvasEditorViewRenderer))]
namespace Svg.Editor.Forms.UWP
{
    public class SvgCanvasEditorViewRenderer : SKCanvasViewRendererBase<SvgCanvasEditorView, SkiaSharp.Views.UWP.SKXamlCanvasX>
    {
        private UwpGestureRecognizer _gestureRecognizer;

        protected override void OnElementChanged(ElementChangedEventArgs<SvgCanvasEditorView> e)
        {
            base.OnElementChanged(e);

            _gestureRecognizer = new UwpGestureRecognizer(Control);
            Engine.RegisterSingleton<IGestureRecognizer, UwpGestureRecognizer>(() => _gestureRecognizer);

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

            Element.DrawingCanvas = new SvgDrawingCanvas();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _gestureRecognizer.Dispose();
        }
    }
}
