using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class ZoomTool : ToolBase
    {
        private readonly SvgDrawingCanvas _owner;
        private ScaleEvent _scaleEvent = null;
        private float _lastFocusX;
        private float _lastFocusY;
       
        public ZoomTool(SvgDrawingCanvas owner, float minScale = 0.5f, float maxScale = 5f)
            :base("Zoom")
        {
            _owner = owner;
            MinScale = minScale;
            MaxScale = maxScale;

            Commands = new List<IToolCommand>()
            {
                new ToolCommand(this, "Zoom in +", (x) =>
                {
                    var f =_owner.ZoomFactor + 0.25f;
                    _owner.ZoomFactor = owner.ZoomFactor = Math.Max(MinScale, Math.Min(f, MaxScale));
                    _owner.InvalidateCanvas();
                }),
                new ToolCommand(this, "Zoom out -", (x) =>
                {
                    var f =_owner.ZoomFactor - 0.25f;
                    _owner.ZoomFactor = owner.ZoomFactor = Math.Max(MinScale, Math.Min(f, MaxScale));
                    _owner.InvalidateCanvas();
                })
            };
        }

        public float MinScale { get; set; }

        public float MaxScale { get; set; }

        public override void OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            if (IsScalingInProgress())
            {
                // see implementation at: http://stackoverflow.com/questions/19418878/implementing-pinch-zoom-and-drag-using-androids-build-in-gesture-listener-and-s/19545542#19545542
                var focusX = _scaleEvent.FocusX;
                var focusY = _scaleEvent.FocusY;

                //Zoom focus is where the fingers are centered, 
                var m = renderer.Matrix;
                m.Translate(-focusX, -focusY, MatrixOrder.Append);
                m.Scale(ws.ZoomFactor, ws.ZoomFactor, MatrixOrder.Append);

                /* Adding focus shift to allow for scrolling with two pointers down. Remove it to skip this functionality.
                 *  This could be done in fewer lines, but for clarity I do it this way here */
                float focusShiftX = focusX - _lastFocusX;
                float focusShiftY = focusY - _lastFocusY;
                m.Translate(focusX + focusShiftX, focusY + focusShiftY, MatrixOrder.Append);

                _lastFocusX = focusX;
                _lastFocusY = focusY;

                renderer.Matrix = m;

                //renderer.Matrix = 
                //renderer.Scale(ws.ZoomFactor, focusX, focusY);
                //ws.Translate.X = renderer.Matrix.OffsetX;
                //ws.Translate.Y = renderer.Matrix.OffsetY;
            }
            else
                renderer.Scale(ws.ZoomFactor, 0, 0/*SharedMasterTool.Instance.LastGestureX, SharedMasterTool.Instance.LastGestureY*/);
            //renderer.Scale(ws.ZoomFactor, 0, 0);
            
        }

        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            _scaleEvent = @event as ScaleEvent;
            if (_scaleEvent == null)
                return;

            if (_scaleEvent.Status == ScaleStatus.Start)
            {
                _lastFocusX = _scaleEvent.FocusX;
                _lastFocusY = _scaleEvent.FocusY;
            }
            else if (_scaleEvent.Status == ScaleStatus.Scaling)
            {
                // Don't let the object get too small or too large.
                ws.ZoomFactor = GetBoundedZoomFactor(ws);

                ws.InvalidateCanvas();
            }
        }

        private float GetBoundedZoomFactor(SvgDrawingCanvas ws)
        {
            var newZoomFactor = ws.ZoomFactor*_scaleEvent.ScaleFactor;
            return Math.Max(MinScale, Math.Min(newZoomFactor, MaxScale));
        }

        private bool IsScalingInProgress()
        {
            return _scaleEvent != null && _scaleEvent.Status != ScaleStatus.End;
        }
    }
}
