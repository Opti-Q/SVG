using System.Collections.Generic;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class PanTool : ToolBase
    {
        private SvgDrawingCanvas _owner;

        public PanTool() 
            : base("Pan")
        {
        }

        public override void Initialize(SvgDrawingCanvas ws)
        {
            _owner = ws;

            Commands = new List<IToolCommand>()
            {
                new ToolCommand(this, "Center at 0:0", (x) =>
                {
                    _owner.Translate.X = 0f;
                    _owner.Translate.Y = 0f;
                    _owner.FireInvalidateCanvas();
                }),
            };
        }

        public override void OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            renderer.Translate(ws.Translate.X, ws.Translate.Y);
        }

        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            if (!IsActive)
                return;

            var ev = @event as MoveEvent;

            if (ev == null)
                return;

            ws.Translate.X += ev.RelativeDelta.X;
            ws.Translate.Y += ev.RelativeDelta.Y;
            ws.FireInvalidateCanvas();
        }
    }
}
