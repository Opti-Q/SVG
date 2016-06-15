using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;

namespace Svg.Core.Tools
{
    public class PanTool : ToolBase
    {
        private readonly SvgDrawingCanvas _owner;

        public PanTool(SvgDrawingCanvas owner) 
            : base("Pan")
        {
            _owner = owner;

            Commands = new List<IToolCommand>()
            {
                new ToolCommand(this, "Center at 0:0", (x) =>
                {
                    _owner.Translate.X = 0f;
                    _owner.Translate.Y = 0f;
                    _owner.InvalidateCanvas();
                }),
            };
        }

        public override void OnPreDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            renderer.Translate(ws.Translate.X, ws.Translate.Y);
        }

        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
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
