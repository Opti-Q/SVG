using System;
using System.Collections.Generic;
using MvvmCross.Core.ViewModels;
using Svg.Core;
using Svg.Core.Interfaces;
using Svg.Core.Services;
using Svg.Core.Tools;
using Svg.Droid.SampleEditor.Core.Tools;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core.ViewModels
{
    public class EditorViewModel
        : MvxViewModel
    {
        public SvgDrawingCanvas Canvas { get; set; }

        public void Init()
        {
            // modify tool so that 
            //var tool = Canvas.Tools.OfType<AddRandomItemTool>().FirstOrDefault();
            //if (tool == null)
            //{
            //    tool = new AddRandomItemTool(Canvas);
            //    Canvas.Tools.Add(tool);
            //}
            //// this surely creates a memory leak!!
            //tool.SourceProvider = GetSource;
            //Canvas.Tools.Add(new AuxiliaryLineTool()); // auxiliar line tool
            //Canvas.Tools.Add(new SaveTool(false));
            //Canvas.Tools.Add(new PlaceAsBackgroundTool(null, Engine.Resolve<IUndoRedoService>()));

            Engine.Register<IPickImageService, MvxPickImageService>(() => new MvxPickImageService());

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
                { "linestyles", new [] { "normal", "dashed" } },
                { "linestylenames", new [] { "-----", "- - -" } }
            };

            var freeDrawToolProperties = new Dictionary<string, object>
            {
                { "linestyles", new [] { "normal", "dashed" } },
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

            //var zoomToolProperties = JsonConvert.SerializeObject(new Dictionary<string, object>
            //{
            //    { "minscale", 1.0f },
            //    { "maxscale", 5.0f }
            //}, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            var zoomToolProperties = new Dictionary<string, object>();

            var panToolProperties = new Dictionary<string, object>();

            //var fs = Engine.Resolve<IFileSystem>();
            //var path = fs.PathCombine(fs.GetDefaultStoragePath(), "background.png");

            var placeAsBackgroundToolProperties = new Dictionary<string, object>
            {
                //{ PlaceAsBackgroundTool.ImagePathKey, path },
                { PlaceAsBackgroundTool.ChooseBackgroundEnabledKey, true }
            };

            Engine.Register<ToolFactoryProvider, ToolFactoryProvider>(() => new ToolFactoryProvider(new Func<ITool>[]
            {
                () => new GridTool(gridToolProperties, Engine.Resolve<IUndoRedoService>()),
                () => new MoveTool(Engine.Resolve<IUndoRedoService>()),
                () => new PanTool(panToolProperties),
                () => new RotationTool(null, Engine.Resolve<IUndoRedoService>()),
                () => new ZoomTool(zoomToolProperties),
                () => new SelectionTool(Engine.Resolve<IUndoRedoService>()),
                () => new TextTool(textToolProperties, Engine.Resolve<IUndoRedoService>()),
                () => new LineTool(lineToolProperties, Engine.Resolve<IUndoRedoService>()),
                () => new FreeDrawingTool(freeDrawToolProperties, Engine.Resolve<IUndoRedoService>()),
                () => new ColorTool(colorToolProperties, Engine.Resolve<IUndoRedoService>()),
                () => new StrokeStyleTool(Engine.Resolve<IUndoRedoService>()),
                () => new UndoRedoTool(Engine.Resolve<IUndoRedoService>()),
                () => new ArrangeTool(Engine.Resolve<IUndoRedoService>()),
                () => new AuxiliaryLineTool(),
                () => new SaveTool(false),
                () => new PlaceAsBackgroundTool(placeAsBackgroundToolProperties, Engine.Resolve<IUndoRedoService>()),
                () => new AddRandomItemTool(Canvas) {SourceProvider = GetSource}
            }));

            #endregion

            Canvas = new SvgDrawingCanvas { IsDebugEnabled = true };

            //Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("svg/svg_80ae394472b24f3daaaca4d067656058_78c.svg"));
            //Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("svg/ground_floor_plan.svg"));
        }

        private ISvgSource GetSource(string source)
        {
            return Engine.Resolve<ISvgSourceFactory>().Create(source);
        }
    }
}
