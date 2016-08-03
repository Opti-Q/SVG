using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public interface IColorInputService
    {
        Task<int> GetIndexFromUserInput(string title, string[] items);
    }

    public class ColorTool : ToolBase
    {
        private static IColorInputService ColorInputServiceProxy => Engine.Resolve<IColorInputService>();

        public ColorTool() : base("Color")
        {
        }

        public string ColorIconName { get; set; } = "ic_format_color_fill_white_48px.svg";

        public Color[] SelectableColors { get; set; } = {
            Color.Create(0, 0, 0),
            Color.Create(255, 0, 0),
            Color.Create(0, 255, 0),
            Color.Create(0, 0, 255),
            Color.Create(255, 255, 0),
            Color.Create(255, 0, 255),
            Color.Create(0, 255, 255)
        };

        public Color SelectedColor { get; set; }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            SelectedColor = SelectableColors?.FirstOrDefault();

            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ChangeColorCommand(ws, this, "Change color")
            };

            // initialize with callbacks
            WatchDocument(ws.Document);

            return Task.FromResult(true);
        }

        private static void ColorizeElement(SvgElement element, Color color)
        {
            var colourServer = new SvgColourServer(color);

            foreach (var child in element.Children)
            {
                ColorizeElement(child, color);
            }

            // only colorize texts and paths
            if (!(element is SvgPath || element is SvgText)) return;

            if (element is SvgText)
            {
                element.Fill?.Dispose();
                element.Fill = colourServer;
            }

            element.Stroke?.Dispose();
            element.Stroke = colourServer;
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
                : base(tool, name, o => { }, iconName: tool.ColorIconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            public override async void Execute(object parameter)
            {
                var t = (ColorTool)Tool;

                var colorNames = new[] { "Black", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan" };
                var color = t.SelectableColors[await ColorInputServiceProxy.GetIndexFromUserInput("Choose color", colorNames)];

                if (_canvas.SelectedElements.Any())
                {
                    // change the color of all selected items
                    foreach (var selectedElement in _canvas.SelectedElements)
                    {
                        ColorizeElement(selectedElement, color);
                    }
                    _canvas.FireInvalidateCanvas();
                    // don't change the global color when items are selected
                    return;
                }

                t.SelectedColor = color;
                _canvas.FireToolCommandsChanged();
            }
        }
    }
}