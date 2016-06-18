using System;
using System.Collections.Generic;
using System.Diagnostics;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class ZoomTool : ToolBase
    {
        private SvgDrawingCanvas _owner;
       
        public ZoomTool(float minScale = 0.5f, float maxScale = 5f)
            :base("Zoom")
        {
            MinScale = minScale;
            MaxScale = maxScale;
        }

        public float MinScale { get; set; }

        public float MaxScale { get; set; }

        public override void Initialize(SvgDrawingCanvas ws)
        {
            _owner = ws;

            Commands = new List<IToolCommand>()
            {
                new ToolCommand(this, "Zoom in +", (x) =>
                {
                    var f =_owner.ZoomFactor + 0.25f;
                    _owner.ZoomFactor = ws.ZoomFactor = Math.Max(MinScale, Math.Min(f, MaxScale));
                    _owner.FireInvalidateCanvas();
                }),
                new ToolCommand(this, "Zoom out -", (x) =>
                {
                    var f =_owner.ZoomFactor - 0.25f;
                    _owner.ZoomFactor = ws.ZoomFactor = Math.Max(MinScale, Math.Min(f, MaxScale));
                    _owner.FireInvalidateCanvas();
                })
            };
        }
        
        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return;

            var se = @event as ScaleEvent;
            if (se == null)
                return;

            if (se.Status == ScaleStatus.Scaling)
            {
                // Don't let the object get too small or too large.
                ws.ZoomFactor = GetBoundedZoomFactor(se, ws);
                ws.FireInvalidateCanvas();
            }
        }

        private float GetBoundedZoomFactor(ScaleEvent se, SvgDrawingCanvas ws)
        {
            var newZoomFactor = ws.ZoomFactor * se.ScaleFactor;
            return Math.Max(MinScale, Math.Min(newZoomFactor, MaxScale));
        }
    }
}
