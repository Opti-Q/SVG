using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Interfaces;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public interface IColorInputService
    {
        Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors);
    }

    public class ColorTool : ToolBase
    {
        private static IColorInputService ColorInputServiceProxy => Engine.Resolve<IColorInputService>();

        public ColorTool() : base("Color")
        {
            IconName = "svg/ic_format_color_fill_white_48px.svg";
            Properties.Add("selectablecolors", new[]
            {
                "#000000",
                "#FF0000",
                "#00FF00",
                "#0000FF",
                "#FFFF00",
                "#FF00FF",
                "#00FFFF"
            });
        }

        public string[] SelectableColors
        {
            get
            {
                object selectableColors;
                Properties.TryGetValue("selectablecolors", out selectableColors);
                if (selectableColors == null) selectableColors = Enumerable.Empty<Color>();
                return (string[])selectableColors;
            }
        }

        public Color SelectedColor { get; set; }

        public string ColorIconNameModifier => StringifyColor(SelectedColor);

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            var selectableColors = SelectableColors;

            SelectedColor = Color.Create(SelectableColors?.FirstOrDefault());

            // cache icons
            var cachingService = Engine.TryResolve<ISvgCachingService>();
            if (cachingService != null)
            {
                foreach (var selectableColor in selectableColors)
                {
                    var color = Color.Create(selectableColor);
                    Action<SvgDocument> action =
                        document =>
                        {
                            document.Children.Single().Children.Last().Fill = new SvgColourServer(color);
                        };
                    cachingService.SaveAsPng(IconName, StringifyColor(color), action);
                }
            }

            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ChangeColorCommand(ws, this, "Change color")
            };

            // initialize with callbacks
            WatchDocument(ws.Document);

            return Task.FromResult(true);
        }

        private static string StringifyColor(Color color)
        {
            return $"{color.R}_{color.G}_{color.B}";
        }

        private static void ColorizeElement(SvgElement element, Color color)
        {
            var colourServer = new SvgColourServer(color);

            foreach (var child in element.Children)
            {
                ColorizeElement(child, color);
            }

            // only colorize visual elements
            if (!(element is SvgVisualElement)) return;

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
                : base(tool, name, o => { }, iconName: tool.IconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            public override async void Execute(object parameter)
            {
                var t = (ColorTool)Tool;

                var colorNames = new[] { "Black", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan" };
                var color = Color.Create(t.SelectableColors[await ColorInputServiceProxy.GetIndexFromUserInput("Choose color", colorNames, t.SelectableColors)]);

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