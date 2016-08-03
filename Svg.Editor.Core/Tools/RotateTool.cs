using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public class RotateTool : ToolBase
    {
        private bool _wasImplicitlyActivated = false;

        public RotateTool() : base("Rotate")
        {
        }

        public override Task OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            var re = @event as RotateEvent;

            // if a "RotateEvent" comes in
            if (re != null)
            {
                var zt = ws.Tools.OfType<ZoomTool>().Single();

                if (re.Status == RotateStatus.Start &&
                    // and there is a single selected element
                    ws.SelectedElements.Count == 1 &&
                    // and the selectiontool is active
                    ws.ActiveTool is SelectionTool)
                {
                    // implicitly activate
                    ws.ActiveTool = this;
                    _wasImplicitlyActivated = true;
                    zt.IsActive = false;
                }
                else if (re.Status == RotateStatus.Rotating &&
                         ws.SelectedElements.Count == 1)
                {
                    RotateElement(ws.SelectedElements[0], re, ws);
                }
                else if(re.Status == RotateStatus.End)
                {
                    if (ws.ActiveTool == this && _wasImplicitlyActivated)
                    {
                        ws.ActiveTool = ws.Tools.OfType<SelectionTool>().Single();
                    }
                    zt.IsActive = true;
                }
            }
            
            return Task.FromResult(true);
        }

        private void RotateElement(SvgVisualElement element, RotateEvent rotateEvent, SvgDrawingCanvas ws)
        {
            var rotateTrans = element.Transforms.OfType<SvgRotate>().LastOrDefault();
            if (rotateTrans == null)
            {
                rotateTrans = new SvgRotate(rotateEvent.AbsoluteRotationDegrees);
                element.Transforms.Add(rotateTrans);
            }
            else
            {
                rotateTrans.Angle += rotateEvent.RelativeRotationDegrees;
            }

            ws.FireInvalidateCanvas();
        }
    }
}
