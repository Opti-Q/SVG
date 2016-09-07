using System;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public class PanTool : ToolBase
    {
        public PanTool(string jsonProperties)
            : base("Pan", jsonProperties)
        {
            IconName = "ic_pan_tool_white_48dp.png";
            ToolType = ToolType.View;
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return Task.FromResult(true);

            var ev = @event as MoveEvent;

            if (ev == null || ev.PointerCount != 2)
                return Task.FromResult(true);

            ws.Translate.X += ev.RelativeDelta.X;
            ws.Translate.Y += ev.RelativeDelta.Y;
            ws.FireInvalidateCanvas();

            ws.FireInvalidateCanvas();

            return Task.FromResult(true);
        }
    }
}
