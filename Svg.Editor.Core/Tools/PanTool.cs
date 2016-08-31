using System.Threading.Tasks;
using Svg.Core.Events;

namespace Svg.Core.Tools
{
    public class PanTool : ToolBase
    {
        public PanTool() 
            : base("Pan")
        {
            IconName = "ic_pan_tool_white_48dp.png";
            //ToolUsage = ToolUsage.Explicit;
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

            return Task.FromResult(true);
        }
    }
}
