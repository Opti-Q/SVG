using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _handledPointerDown;

        public string DeleteIconName { get; set; } = "ic_delete_white_48dp.png";
        public string SelectIconName { get; set; } = "ic_select_tool_white_48dp.png";

        public SelectionTool() : base("Select")
        {
            this.IconName = SelectIconName;
            this.ToolUsage = ToolUsage.Explicit;
        }

        private Brush BlueBrush => _brush ?? (_brush = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = Svg.Engine.Factory.CreatePen(BlueBrush, 5));

        public override bool IsActive
        {
            get { return base.IsActive; }
            set
            {
                base.IsActive = value;
                Reset();
            }
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Delete", (o) =>
                {
                    foreach (var element in ws.SelectedElements)
                    {
                        element.Parent.Children.Remove(element);
                    }
                    ws.SelectedElements.Clear();
                    ws.FireToolCommandsChanged();
                    ws.FireInvalidateCanvas();
                }, 
                (o) => ws.SelectedElements.Any(), iconName:DeleteIconName, 
                sortFunc: (t) => 550)
            };

            return Task.FromResult(true);
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return Task.FromResult(true);

            var e = @event as MoveEvent;
            if (e != null && _handledPointerDown && e.PointerCount == 1)
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
                var rect = RectangleF.Create(startX, startY, endX - startX, endY - startY);
                
                // selection onyl counts if width and height are not too small
                var dist = Math.Sqrt(Math.Pow(rect.Width, 2) + Math.Pow(rect.Height, 2));

                if (dist > 30)
                {
                    _selectionRectangle = rect;
                    ws.FireInvalidateCanvas();
                }
            }

            var p = @event as PointerEvent;
            if (p != null)
            {
                if (p.EventType == EventType.PointerDown)
                {
                    _handledPointerDown = p.PointerCount == 1;
                }
                // if the user never moved, but clicked on an item, we try to select that spot
                if (_handledPointerDown && p.EventType == EventType.PointerUp && _selectionRectangle == null)
                {
                    // select elements under pointer
                    SelectElementsUnder(ws.GetPointerRectangle(p.Pointer1Position), ws, SelectionType.Intersect, 1);
                    Reset();
                    ws.FireInvalidateCanvas();
                }
                // on pointer up or cancel, we remove the selection rectangle
                else if (_handledPointerDown && p.EventType == EventType.PointerUp || p.EventType == EventType.Cancel)
                {
                    // select elements under rectangle
                    SelectElementsUnder(_selectionRectangle, ws, SelectionType.Contain);

                    Reset();
                    ws.FireInvalidateCanvas();
                }
            }

            return Task.FromResult(true);
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            _selectionRectangle = null;
        }

        private void SelectElementsUnder(RectangleF selectionRectangle, SvgDrawingCanvas ws, SelectionType selectionType, int maxItems = int.MaxValue)
        {
            ws.SelectedElements.Clear();

            // the canvas has not been scaled and translated yet
            // we need to compare our rectangle to the translated boundingboxes of the svg elements
            var selected = ws.GetElementsUnder<SvgVisualElement>(selectionRectangle, selectionType, maxItems);

            foreach (var element in selected)
                ws.SelectedElements.Add(element);
        }

        public override Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            // we draw the selection rectangle
            if (_selectionRectangle != null)
            {
                renderer.Graphics.Save();

                // as we are in "OnDraw", an panning as well as zoomin tools have already translated 
                // the canvas in order to properly render the svg elements zoomed and panned
                // we need to undo that translation, as our selection rectangle must be drawn
                // in absolute screen coordinates (below the finger of the user - not translated and scaled)
                var m = renderer.Graphics.Transform.Clone();
                m.Invert();
                renderer.Graphics.Concat(m);
                renderer.DrawRectangle(_selectionRectangle, BluePen);

                renderer.Graphics.Restore();
            }


            // we draw the selection boundingboxes of all selected elements
            foreach (var element in ws.SelectedElements)
            {
                renderer.Graphics.Save();

                // we draw a selection adorner around all elements
                // as the canvas is already translated and scaled, we just need the plai boundingbox (as poopsed to transformed one using element.GetBoundingBox(ws.GetCanvasTransformationMatrix()))
                renderer.DrawRectangle(element.GetBoundingBox(), BluePen);

                renderer.Graphics.Restore();
            }

            return Task.FromResult(true);
        }

        private void Reset()
        {
            _handledPointerDown = false;
            _selectionRectangle = null;
        }
    }
}
