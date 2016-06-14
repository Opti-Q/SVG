using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class PanTool : ToolBase
    {
        public PanTool() 
            : base("Pan")
        {
        }

        public override void OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            renderer.Translate(ws.Translate.X, ws.Translate.Y);
        }

        public override void OnTouch(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            var ev = @event as MoveEvent;

            if (ev == null)
                return;

            ws.Translate.X += ev.AbsoluteDelta.X / ws.ZoomFactor;
            ws.Translate.Y += ev.AbsoluteDelta.Y / ws.ZoomFactor;
            System.Diagnostics.Debug.WriteLine($"{ws.Translate.X}:{ws.Translate.Y}");

            ws.InvalidateCanvas();
        }
    }
}
