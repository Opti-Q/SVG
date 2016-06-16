using System;
using System.Collections.Generic;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class ZoomTool : ToolBase
    {
        private readonly SvgDrawingCanvas _owner;
       
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
            //renderer.Scale(ws.ZoomFactor, 0f, 0f);
            var m = renderer.Matrix;
            m.Scale(ws.ZoomFactor, ws.ZoomFactor, MatrixOrder.Append);
            renderer.Matrix = m;
        }

        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            var se = @event as ScaleEvent;

            if (se?.Status == ScaleStatus.Scaling)
            {
                // Don't let the object get too small or too large.
                ws.ZoomFactor = GetBoundedZoomFactor(se, ws);

                ws.InvalidateCanvas();
            }
        }

        private float GetBoundedZoomFactor(ScaleEvent se, SvgDrawingCanvas ws)
        {
            var newZoomFactor = ws.ZoomFactor * se.ScaleFactor;
            return Math.Max(MinScale, Math.Min(newZoomFactor, MaxScale));
        }
    }
}
