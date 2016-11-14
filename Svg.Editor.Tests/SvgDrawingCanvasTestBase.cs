using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Svg.Droid.SampleEditor.Core.Tools;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    public abstract class SvgDrawingCanvasTestBase
    {
        private SvgDrawingCanvas _canvas;

        protected SvgDrawingCanvas Canvas => _canvas;
        protected SchedulerProvider SchedulerProvider { get; } = new SchedulerProvider(CurrentThreadScheduler.Instance, new TestScheduler());

        [SetUp]
        public virtual void SetUp()
        {
            Engine.Register<SchedulerProvider, SchedulerProvider>(() => SchedulerProvider);

            #region Register tools

            // this part should be in the designer, when the iCL is created
            var gridToolProperties = new Dictionary<string, object>
            {
                { "angle", 30.0f },
                { "stepsizey", 20.0f },
                { "issnappingenabled", true }
            };

            var colorToolProperties = new Dictionary<string, object>
            {
                { "selectablecolors", new [] { "#000000","#FF0000","#00FF00","#0000FF","#FFFF00","#FF00FF","#00FFFF" } }
            };

            var lineToolProperties = new Dictionary<string, object>
            {
                { "markerstartids", new [] { "none", "arrowStart", "circle" } },
                { "markerstartnames", new [] { "---", "<--", "O--" } },
                { "markerendids", new [] { "none", "arrowEnd", "circle" } },
                { "markerendnames", new [] { "---", "-->", "--O" } },
                { "linestyles", new [] { "none", "3 3" } },
                { "linestylenames", new [] { "-----", "- - -" } }
            };

            var freeDrawToolProperties = new Dictionary<string, object>
            {
                { "linestyles", new [] { "none", "3 3" } },
                { "linestylenames", new [] { "-----", "- - -" } },
                { "strokewidths", new [] { 12, 24, 6 } },
                { "strokewidthnames", new [] { "normal", "thick", "thin" } }
            };

            var textToolProperties = new Dictionary<string, object>
            {
                { "fontsizes", new [] { 12f, 16f, 20f, 24f, 36f, 48f } },
                { "selectedfontsizeindex", 1 },
                { "fontsizenames", new [] { "12px", "16px", "20px", "24px", "36px", "48px" } }
            };

            var strokeStyleToolProperties = new Dictionary<string, object>
            {
                { StrokeStyleTool.StrokeDashesKey, new[] {"none", "3 3", "17 17", "34 34"} },
                { StrokeStyleTool.StrokeDashNamesKey, new [] { "----------", "- - - - - -", "--  --  --", "---   ---" } },
                { StrokeStyleTool.StrokeWidthsKey, new [] { 2, 6, 12, 24 } },
                { StrokeStyleTool.StrokeWidthNamesKey, new [] { "thin", "normal", "thick", "ultra-thick" } }
            };

            //var fs = Engine.Resolve<IFileSystem>();
            //var path = fs.PathCombine(fs.GetDefaultStoragePath(), "background.png");

            var placeAsBackgroundToolProperties = new Dictionary<string, object>
            {
                //{ PlaceAsBackgroundTool.ImagePathKey, path },
                { PlaceAsBackgroundTool.ChooseBackgroundEnabledKey, true }
            };

            var undoRedoService = Engine.Resolve<IUndoRedoService>();

            Engine.Register<ToolFactoryProvider, ToolFactoryProvider>(() => new ToolFactoryProvider(new Func<ITool>[]
            {
                () => new GridTool(gridToolProperties, undoRedoService),
                () => new MoveTool(undoRedoService),
                () => new PanTool(null),
                () => new RotationTool(null, undoRedoService),
                () => new ZoomTool(null),
                () => new SelectionTool(undoRedoService),
                () => new TextTool(textToolProperties, undoRedoService),
                () => new LineTool(lineToolProperties, undoRedoService),
                () => new FreeDrawingTool(freeDrawToolProperties, undoRedoService),
                () => new ColorTool(colorToolProperties, undoRedoService),
                () => new StrokeStyleTool(strokeStyleToolProperties ,undoRedoService),
                () => new UndoRedoTool(undoRedoService),
                () => new ArrangeTool(undoRedoService),
                () => new AuxiliaryLineTool(),
                () => new SaveTool(false),
                () => new PlaceAsBackgroundTool(placeAsBackgroundToolProperties, undoRedoService)
            }));

            #endregion

            _canvas = new SvgDrawingCanvas();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _canvas.Dispose();
        }

        protected SvgDocument LoadDocument(string fileName)
        {
            var l = Engine.Resolve<IFileLoader>();
            return l.Load(fileName);
        }
        
        protected async Task Rotate(params float[] relativeAnglesDegree)
        {
            await Canvas.OnEvent(new RotateEvent(0, 0, RotateStatus.Start, 3));

            var sum = 0f;
            foreach (var a in relativeAnglesDegree)
            {
                sum += a;
                await Canvas.OnEvent(new RotateEvent(a, sum, RotateStatus.Rotating, 3));
            }

            await Canvas.OnEvent(new RotateEvent(0, sum, RotateStatus.End, 0));
        }

        protected async Task Move(PointF start, PointF end, int pointerCount = 1)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, pointerCount));
            var delta = end - start;
            await Canvas.OnEvent(new MoveEvent(start, start, end, delta, pointerCount));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, start, start, end, pointerCount));
        }
    }
}
