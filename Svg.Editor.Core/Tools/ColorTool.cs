using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Editor.Utils;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
    public interface IColorInputService
    {
        Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors);
    }

    public class ColorTool : UndoableToolBase
    {
        #region Private fields and properties

        private static IColorInputService ColorInputService => Engine.Resolve<IColorInputService>();

        private static ISvgCachingService SvgCachingService => Engine.Resolve<ISvgCachingService>();

        private static IFileSystem FileSystemService => Engine.Resolve<IFileSystem>();

        private Color _defaultSelectedColor;

        #endregion

        #region Public properties

        public const string SelectedColorIndexKey = "selectedcolorindex";
        public const string SelectableColorsKey = "selectablecolors";
        public const string SelectableColorNamesKey = "selectablecolornames";

        public string[] SelectableColors
        {
            get
            {
                object selectableColors;
                if (!Properties.TryGetValue(SelectableColorsKey, out selectableColors))
                    selectableColors = Enumerable.Empty<string>();
                return (string[]) selectableColors;
            }
        }

        public string[] SelectableColorNames
        {
            get
            {
                object selectableColorNames;
                if (!Properties.TryGetValue(SelectableColorNamesKey, out selectableColorNames))
                    selectableColorNames = SelectableColors.Clone();
                return (string[]) selectableColorNames;
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

        #endregion

        public ColorTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Color", properties, undoRedoService)
        {
            IconName = "svg/ic_format_color_fill_white_48px.svg";
            ToolType = ToolType.Modify;

            object selectedColorIndex;
            SelectedColor = Properties.TryGetValue(SelectedColorIndexKey, out selectedColorIndex)
                ? Color.Create(SelectableColors.ElementAtOrDefault(Convert.ToInt32(selectedColorIndex)) ?? "#000000")
                : Color.Create(SelectableColors.FirstOrDefault() ?? "#000000");
        }

        #region Overrides

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            // cache icons
            var cachingService = Engine.TryResolve<ISvgCachingService>();
            if (cachingService != null)
            {
                foreach (var selectableColor in SelectableColors)
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

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            // add watch for global colorizing
            WatchDocument(newDocument);
            UnWatchDocument(oldDocument);
        }

        #endregion

        #region Private helpers

        private static string StringifyColor(Color color)
        {
            return $"{color.R}_{color.G}_{color.B}";
        }

        private void ColorizeElement(SvgElement element, Color color)
        {
            var noFill = element.HasConstraints(NoFillConstraint);
            var noStroke = element.HasConstraints(NoStrokeConstraint);

            // only colorize visual elements
            if (!(element is SvgVisualElement) || noFill && noStroke) return;

            var oldStroke = ((SvgColourServer) element.Stroke)?.ToString();
            var oldFill = ((SvgColourServer) element.Fill)?.ToString();
            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize element", _ =>
            {
                if (!noStroke)
                {
                    element.Stroke?.Dispose();
                    element.Stroke = new SvgColourServer(color);
                }
                if (!noFill)
                {
                    element.Fill?.Dispose();
                    element.Fill = new SvgColourServer(color);
                }
                Canvas.FireInvalidateCanvas();
            }, _ =>
            {
                if (!noStroke)
                {
                    element.Stroke?.Dispose();
                    element.SvgElementFactory.SetPropertyValue(element, "stroke", oldStroke, element.OwnerDocument);
                }
                if (!noFill)
                {
                    element.Fill?.Dispose();
                    element.SvgElementFactory.SetPropertyValue(element, "fill", oldFill, element.OwnerDocument);
                }
                Canvas.FireInvalidateCanvas();
            }), hasOwnUndoRedoScope: false);
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

        #endregion

        #region Inner types

        /// <summary>
        /// This command changes the color of selected items, or the global selected color, if no items are selected.
        /// </summary>
        private class ChangeColorCommand : ToolCommand
        {
            private readonly ISvgDrawingCanvas _canvas;

            public ChangeColorCommand(ISvgDrawingCanvas canvas, ColorTool tool, string name)
                : base(tool, name, o => { }, iconName: tool.IconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            private new ColorTool Tool => (ColorTool) base.Tool;

            public override async void Execute(object parameter)
            {
                var t = Tool;

                var color = Color.Create(t.SelectableColors[await ColorInputService.GetIndexFromUserInput("Choose color", t.SelectableColorNames, t.SelectableColors)]);

                if (_canvas.SelectedElements.Any())
                {
                    t.UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize selected elements", o => { }));
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

            public override string IconName => SvgCachingService.GetCachedPngPath(Tool.IconName, StringifyColor(Tool.SelectedColor), FileSystemService);
        }

        #endregion
    }
}