using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Core.Events;

namespace Svg.Core.Tools
{
    public class SelectionTool : ToolBase
    {
        public SelectionTool() : base("Select")
        {
        }

        public override void Initialize(SvgDrawingCanvas ws)
        {
            Commands = new List<IToolCommand>
            {
                new ToggleSelectionToolCommand(this, ws)
            };

            // make sure selection is inactive in case that panning is active at start
            this.IsActive = !ws.Tools.OfType<PanTool>().FirstOrDefault()?.IsActive ?? true;
        }

        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return;

            var e = @event as PointerEvent;
            if (e == null)
                return;


            
        }

        private class ToggleSelectionToolCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ToggleSelectionToolCommand(SelectionTool tool, SvgDrawingCanvas canvas) : base(tool, "Select", (obj)=> {})
            {
                _canvas = canvas;
            }

            public override void Execute(object parameter)
            {
                Tool.IsActive = !Tool.IsActive;
                var panTool = _canvas.Tools.OfType<PanTool>().FirstOrDefault();
                if (panTool != null)
                    panTool.IsActive = !Tool.IsActive;

                Name = this.Tool.IsActive ? "Pan" : "Select";

                _canvas.FireToolCommandsChanged();
            }

            public override bool CanExecute(object parameter)
            {
                return true;
            }
        }
    }
}
