﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
    public class SelectionTool : UndoableToolBase
    {
        #region Private fields

        private RectangleF _selectionRectangle;
        private Brush _brush;
        private Pen _pen;
        private bool _handledPointerDown;

        #endregion

        #region Private properties

        private string DeleteIconName { get; } = "ic_delete_white_48dp.png";
        private string SelectIconName { get; } = "ic_select_tool_white_48dp.png";
        private Brush BlueBrush => _brush ?? (_brush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = Engine.Factory.CreatePen(BlueBrush, 5));

        #endregion

        #region Public properties

        public override int GestureOrder => 1000;

        public override bool IsActive
        {
            get { return base.IsActive; }
            set
            {
                base.IsActive = value;
                Reset();
            }
        }

        #endregion

        public SelectionTool(IUndoRedoService undoRedoService) : base("Select", undoRedoService)
        {
            IconName = SelectIconName;
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Select;
            HandleDragExit = true;
        }

        #region Overrides

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Delete", o =>
                {
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Remove operation", x => {}));
                        foreach (var element in ws.SelectedElements)
                        {
                            var parent = element.Parent;
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Remove element", x =>
                            {
                                parent.Children.Remove(element);
                                ws.FireInvalidateCanvas();
                            }, x =>
                            {
                                parent.Children.Add(element);
                                ws.FireInvalidateCanvas();
                            }), hasOwnUndoRedoScope: false);
                        }
                        ws.SelectedElements.Clear();
                        ws.FireInvalidateCanvas();
                },
                o => ws.SelectedElements.Any(), iconName:DeleteIconName,
                sortFunc: t => 550)
            };
        }

        protected override async Task OnTap(TapGesture tap)
        {
            await base.OnTap(tap);

            if (!IsActive) return;

            // select elements under pointer
            SelectElementsUnder(Canvas.GetPointerRectangle(tap.Position), Canvas, SelectionType.Intersect, 1);

            Reset();
            Canvas.FireInvalidateCanvas();
        }

        protected override async Task OnDrag(DragGesture drag)
        {
            await base.OnDrag(drag);

            if (!IsActive) return;

            if (drag.State == DragState.Exit)
            {
                // select elements under rectangle
                if (_selectionRectangle != null)
                    SelectElementsUnder(_selectionRectangle, Canvas, SelectionType.Contain);

                Reset();
                Canvas.FireInvalidateCanvas();

                return;
            }

            var location = drag.Start;
            var size = drag.Delta;

            if (size.Width < 0 && size.Height < 0)
            {
                location = location + size;
                size = SizeF.Create(Math.Abs(size.Width), Math.Abs(size.Height));
            }
            else if (size.Height < 0)
            {
                location = PointF.Create(location.X, location.Y + size.Height);
                size = SizeF.Create(size.Width, Math.Abs(size.Height));
            }
            else if (size.Width < 0)
            {
                location = PointF.Create(location.X + size.Width, location.Y);
                size = SizeF.Create(Math.Abs(size.Width), size.Height);
            }

            _selectionRectangle = RectangleF.Create(location, size);

            Canvas.FireInvalidateCanvas();
        }

        public override Task OnDraw(IRenderer renderer, ISvgDrawingCanvas ws)
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

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            _selectionRectangle = null;
        }

        public override void Reset()
        {
            _handledPointerDown = false;
            _selectionRectangle = null;
        }

        #endregion

        #region Private helpers

        private static void SelectElementsUnder(RectangleF selectionRectangle, ISvgDrawingCanvas ws, SelectionType selectionType, int maxItems = int.MaxValue)
        {
            ws.SelectedElements.Clear();

            // the canvas has not been scaled and translated yet
            // we need to compare our rectangle to the translated boundingboxes of the svg elements
            var selected = ws.GetElementsUnder<SvgVisualElement>(selectionRectangle, selectionType, maxItems);

            foreach (var element in selected)
            {
                if (element.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey)) continue;
                ws.SelectedElements.Add(element);
            }
        }

        #endregion
    }
}
