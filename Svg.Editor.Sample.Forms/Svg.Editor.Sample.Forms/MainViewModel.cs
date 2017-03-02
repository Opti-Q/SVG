using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;

namespace Svg.Editor.Sample.Forms
{
    public class MainViewModel
    {
        public MainViewModel()
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
            DrawingCanvas = drawingCanvas;
        }

        public SvgDrawingCanvas DrawingCanvas { get; set; }
    }
}
