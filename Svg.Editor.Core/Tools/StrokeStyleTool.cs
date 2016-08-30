using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Interfaces;
using Svg.Core.UndoRedo;

namespace Svg.Core.Tools
{
    public class StrokeStyleTool : UndoableToolBase
    {
        public StrokeStyleTool(IUndoRedoService undoRedoService) : base("Stroke style", undoRedoService)
        {
            IconName = "ic_border_style_white_48dp.png";
            ToolType = ToolType.Modify;
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ChangeStrokeStyleCommand(ws, this, "Change stroke")
            };

            return base.Initialize(ws);
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
                    var formerStrokeDashArray = selectedElement.StrokeDashArray;
                    ((StrokeStyleTool) Tool).UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name,
                        o =>
                        {
                            selectedElement.StrokeDashArray = SvgUnitCollection.IsNullOrEmpty(selectedElement.StrokeDashArray) ? "10 10" : null;
                            _canvas.FireInvalidateCanvas();
                        }, o =>
                        {
                            selectedElement.StrokeDashArray = formerStrokeDashArray;
                            _canvas.FireInvalidateCanvas();
                        }));
                }
            }

            public override bool CanExecute(object parameter)
            {
                return _canvas.SelectedElements.Any();
            }
        }
    }
}