using System.Collections.Generic;
using System.Linq;
using Svg.Core.Events;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public class MoveTool : ToolBase
    {
        public MoveTool() : base("Move")
        {
        }

        public override void Initialize(SvgDrawingCanvas ws)
        {
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Move", (obj) =>
                {
                    this.IsActive = !this.IsActive;
                    var panTool = ws.Tools.OfType<PanTool>().FirstOrDefault();
                    if (panTool != null)
                    {
                        // only either pantool or movetool can be active
                        panTool.IsActive = !this.IsActive;
                    }

                }, (obj) => !this.IsActive)
            };
        }
        
        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {

            var p = @event as PointerEvent;
            if (p != null && p.EventType == EventType.PointerDown)
            {
                // determine if active by searching thorough selection and determining whether pointer was put on selected element
                // if there are selected elements and pointer was put down on one of them, activate tool, otherwhise deactivate
                if (ws.SelectedElements.Count != 0 &&
                    ws.GetElementsUnderPointer(p.Pointer1Position).Any(eup => ws.SelectedElements.Contains(eup)))
                {
                    this.IsActive = true;
                }
                else
                {
                    this.IsActive = false;
                }

            }

            // skip moving if inactive
            if (!this.IsActive)
                return;


            var e = @event as MoveEvent;

            if (e != null)
            {
                // if there is no selection, we just skip
                if (ws.SelectedElements.Any())
                {
                    var deltaX = e.RelativeDelta.X / ws.ZoomFactor;
                    var deltaY = e.RelativeDelta.Y / ws.ZoomFactor;

                    // add translation to every element
                    foreach (var elemnt in ws.SelectedElements)
                    {
                        var trans = elemnt.Transforms.OfType<SvgTranslate>().LastOrDefault();
                        if (trans == null)
                        {
                            trans = new SvgTranslate(deltaX, deltaY);
                            elemnt.Transforms.Add(trans);
                        }
                        else
                        {
                            trans.X += deltaX;
                            trans.Y += deltaY;
                        }
                    }   

                    ws.FireInvalidateCanvas();
                }
            }
        }
    }
}
