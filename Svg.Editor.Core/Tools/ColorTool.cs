using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Interfaces;
using Svg.Core.UndoRedo;
using Svg.Core.Utils;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public interface IColorInputService
    {
        Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors);
    }

    public class ColorTool : UndoableToolBase
    {
        private static IColorInputService ColorInputServiceProxy => Engine.Resolve<IColorInputService>();
        private Color _defaultSelectedColor;

        public ColorTool(string properties, IUndoRedoService undoRedoService) : base("Color", properties, undoRedoService)
        {
            IconName = "svg/ic_format_color_fill_white_48px.svg";
            ToolType = ToolType.Modify;
        }

        public string[] SelectableColors
        {
            get
            {
                object selectableColors;
                if (!Properties.TryGetValue("selectablecolors", out selectableColors))
                    selectableColors = Enumerable.Empty<string>();
                return (string[]) selectableColors;
            }
        }

        // implementation for per-tool selected color
        //public Color SelectedColor
        //{
        //    get
        //    {
        //        Color selectedColor;
        //        _selectedColors.TryGetValue(_canvas.ActiveTool?.GetType(), out selectedColor);
        //        return selectedColor ?? _defaultSelectedColor;
        //    }
        //    set
        //    {
        //        if (_canvas.ActiveTool != null && _canvas.ActiveTool.ToolType == ToolType.Create)
        //        {
        //            _selectedColors[_canvas.ActiveTool.GetType()] = value;
        //        }
        //        else
        //        {
        //            _defaultSelectedColor = value;
        //        }
        //    }
        //}

        public Color SelectedColor
        {
            get { return _defaultSelectedColor; }
            set { _defaultSelectedColor = value; }
        }

        public override async Task Initialize(SvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            var selectableColors = SelectableColors;

            SelectedColor = Color.Create(selectableColors.FirstOrDefault() ?? "#000000");

            // cache icons
            var cachingService = Engine.TryResolve<ISvgCachingService>();
            if (cachingService != null)
            {
                foreach (var selectableColor in selectableColors)
                {
                    var color = Color.Create(selectableColor);
                    cachingService.SaveAsPng(IconName, StringifyColor(color), SvgProcessingUtil.ColorAction(color));
                }
            }

            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ChangeColorCommand(ws, this, "Change color")
            };

            // initialize with callbacks
            WatchDocument(ws.Document);
        }

        private static string StringifyColor(Color color)
        {
            return $"{color.R}_{color.G}_{color.B}";
        }

        private void ColorizeElement(SvgElement element, Color color)
        {
            // only colorize visual elements
            if (!(element is SvgVisualElement)) return;

            var oldStroke = ((SvgColourServer) element.Stroke)?.ToString();
            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize stroke", o =>
            {
                element.Stroke?.Dispose();
                element.Stroke = new SvgColourServer(color);
                Canvas.FireInvalidateCanvas();
            }, state =>
            {
                element.Stroke?.Dispose();
                Engine.Resolve<ISvgElementFactory>().SetPropertyValue(element, "stroke", oldStroke, element.OwnerDocument);
                Canvas.FireInvalidateCanvas();
            }), hasOwnUndoRedoScope: false);

            if (element is SvgText || element is SvgLine)
            {
                var oldFill = ((SvgColourServer) element.Fill)?.ToString();
                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize fill", o =>
                {
                    element.Fill?.Dispose();
                    element.Fill = new SvgColourServer(color);
                    Canvas.FireInvalidateCanvas();
                }, state =>
                {
                    element.Fill?.Dispose();
                    Engine.Resolve<ISvgElementFactory>().SetPropertyValue(element, "fill", oldFill, element.OwnerDocument);
                    Canvas.FireInvalidateCanvas();
                }), hasOwnUndoRedoScope: false);
            }
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            // add watch for global colorizing
            WatchDocument(newDocument);
            UnWatchDocument(oldDocument);
        }

        /// <summary>
        /// Subscribes to the documentss "Add/RemoveChild" handlers.
        /// </summary>
        /// <param name="document"></param>
        private void WatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;
            document.ChildAdded += OnChildAdded;
        }

        private void UnWatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;
        }

        private void OnChildAdded(object sender, ChildAddedEventArgs e)
        {
            ColorizeElement(e.NewChild, SelectedColor);
        }

        /// <summary>
        /// This command changes the color of selected items, or the global selected color, if no items are selected.
        /// </summary>
        private class ChangeColorCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ChangeColorCommand(SvgDrawingCanvas canvas, ColorTool tool, string name)
                : base(tool, name, o => { }, iconName: tool.IconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            private new ColorTool Tool => (ColorTool) base.Tool;

            public override async void Execute(object parameter)
            {
                var t = Tool;

                var colorNames = new[] { "Black", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan" };
                var color = Color.Create(t.SelectableColors[await ColorInputServiceProxy.GetIndexFromUserInput("Choose color", colorNames, t.SelectableColors)]);

                if (_canvas.SelectedElements.Any())
                {
                    t.UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize selecte elements", o => {}));
                    // change the color of all selected items
                    foreach (var selectedElement in _canvas.SelectedElements)
                    {
                        t.ColorizeElement(selectedElement, color);
                    }
                    // don't change the global color when items are selected
                    return;
                }

                var formerSelectedColor = t.SelectedColor;
                t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
                {
                    t.SelectedColor = color;
                    t.Canvas.FireToolCommandsChanged();
                }, o =>
                {
                    t.SelectedColor = formerSelectedColor;
                    t.Canvas.FireToolCommandsChanged();
                }));
            }

            public override string IconName
            {
                get
                {
                    var fs = Engine.Resolve<IFileSystem>();
                    var svgCachingService = Engine.Resolve<ISvgCachingService>();
                    var path = svgCachingService.GetCachedPngPath(Tool.IconName, StringifyColor(Tool.SelectedColor), fs);
                    return path;
                }
                set { }
            }
        }
    }
}