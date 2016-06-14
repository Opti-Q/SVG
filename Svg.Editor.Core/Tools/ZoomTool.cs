using System;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class ZoomTool : ToolBase
    {
        private ScaleEvent _scaleEvent = null;
       
        public ZoomTool(float minScale = 0.5f, float maxScale = 5f)
            :base("Zoom")
        {
            MinScale = minScale;
            MaxScale = maxScale;
        }

        public float MinScale { get; set; }

        public float MaxScale { get; set; }

        public override void OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            if (IsScalingInProgress())
                renderer.Scale(ws.ZoomFactor, _scaleEvent.FocusX, _scaleEvent.FocusY);
            else
                renderer.Scale(ws.ZoomFactor, 0f, 0f/*SharedMasterTool.Instance.LastGestureX, SharedMasterTool.Instance.LastGestureY*/);
        }

        public override void OnTouch(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            _scaleEvent = @event as ScaleEvent;
            if (_scaleEvent == null)
                return;

            if (_scaleEvent.Status == ScaleStatus.Scaling)
            {
                var newZoomFactor = ws.ZoomFactor*_scaleEvent.ScaleFactor;

                // Don't let the object get too small or too large.
                ws.ZoomFactor = Math.Max(MinScale, Math.Min(newZoomFactor, MaxScale));

                ws.InvalidateCanvas();
            }
        }
        
        private bool IsScalingInProgress()
        {
            return _scaleEvent != null && _scaleEvent.Status != ScaleStatus.End;
        }
    }
}
