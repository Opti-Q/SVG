using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Svg.Core.Tools
{
    public class StrokeStyleTool : ToolBase
    {
        public StrokeStyleTool() : base("Stroke style")
        {
            IconName = "ic_line_style_white_48dp.png";
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ChangeStrokeStyleCommand(ws, this, "Change stroke")
            };

            // initialize with callbacks
            WatchDocument(ws.Document);

            return Task.FromResult(true);
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
            // TODO: change stroke style of added child
        }

        /// <summary>
        /// This command changes the color of selected items, or the global selected color, if no items are selected.
        /// </summary>
        private class ChangeStrokeStyleCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ChangeStrokeStyleCommand(SvgDrawingCanvas canvas, StrokeStyleTool tool, string name)
                : base(tool, name, o => { }, iconName: tool.IconName, sortFunc: tc => 500)
            {
                _canvas = canvas;
            }

            public override void Execute(object parameter)
            {
                if (!_canvas.SelectedElements.Any()) return;

                // change the stroke style of all selected items
                foreach (var selectedElement in _canvas.SelectedElements)
                {
                    selectedElement.StrokeDashArray = SvgUnitCollection.IsNullOrEmpty(selectedElement.StrokeDashArray) ? "10 10" : null;
                }
                _canvas.FireInvalidateCanvas();
            }

            public override bool CanExecute(object parameter)
            {
                return _canvas.SelectedElements.Any();
            }
        }
    }
}