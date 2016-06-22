using System.Threading.Tasks;
using Svg.Core.Events;

namespace Svg.Core.Tools
{
    public class PanTool : ToolBase
    {
        private SvgDrawingCanvas _owner;

        public PanTool() 
            : base("Pan")
        {
            IconName = "ic_pan_tool_white_48dp.png";
            ToolUsage = ToolUsage.Explicit;
        }

        public override Task Initialize(SvgDrawingCanvas ws)
        {
            _owner = ws;

            //Commands = new List<IToolCommand>()
            //{
            //    new ToolCommand(this, "Center at 0:0", (x) =>
            //    {
            //        _owner.Translate.X = 0f;
            //        _owner.Translate.Y = 0f;
            //        _owner.FireInvalidateCanvas();
            //    }, sort:2000),
            //};

            return Task.FromResult(true);
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return Task.FromResult(true);

            var ev = @event as MoveEvent;

            if (ev == null)
                return Task.FromResult(true);

            ws.Translate.X += ev.RelativeDelta.X;
            ws.Translate.Y += ev.RelativeDelta.Y;
            ws.FireInvalidateCanvas();

            return Task.FromResult(true);
        }
    }
}
