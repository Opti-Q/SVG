using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public class SelectionTool : ToolBase
    {
        private RectangleF _selectionRectangle = null;
        private Brush _brush;
        private Pen _pen;

        public SelectionTool() : base("Select")
        {
        }

        private Brush Brush => _brush ?? (_brush = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen Pen => _pen ?? (_pen = Svg.Engine.Factory.CreatePen(Brush, 1));

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

            var e = @event as MoveEvent;
            if (e != null)
            {
                float startX = e.Pointer1Down.X;
                float startY = e.Pointer1Down.Y;
                float endX = e.Pointer1Position.X;
                float endY = e.Pointer1Position.Y;

                if (startX > endX)
                {
                    var t = startX;
                    startX = endX;
                    endX = t;
                }
                if (startY > endY)
                {
                    var t = startY;
                    startY = endY;
                    endY = t;
                }
                _selectionRectangle = Engine.Factory.CreateRectangleF(startX, startY, endX - startX, endY - startY);

                //Debug.WriteLine($"select: {_selectionRectangle}");
                ws.FireInvalidateCanvas();
            }
        }

        public override void OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            if (_selectionRectangle != null)
            {
                renderer.Graphics.Save();

                var m = renderer.Matrix.Clone();
                m.Invert();
                renderer.Graphics.Concat(m);
                renderer.DrawRectangle(_selectionRectangle, Pen);


                renderer.Graphics.Save();
            }
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
                var selectionTool = (SelectionTool)this.Tool;
                selectionTool.IsActive = !selectionTool.IsActive;

                var panTool = _canvas.Tools.OfType<PanTool>().FirstOrDefault();
                if (panTool != null)
                    panTool.IsActive = !selectionTool.IsActive;

                Name = selectionTool.IsActive ? "Pan" : "Select";

                // also reset selection triangle
                if (!selectionTool.IsActive)
                    selectionTool._selectionRectangle = null;

                _canvas.FireToolCommandsChanged();
            }

            public override bool CanExecute(object parameter)
            {
                return true;
            }
        }
    }
}
