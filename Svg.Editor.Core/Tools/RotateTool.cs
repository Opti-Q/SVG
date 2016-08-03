using System;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Core.Tools
{
    public class RotateTool : ToolBase
    {
        private bool _wasImplicitlyActivated = false;
        private PointF _lastRotationCenter;
        private Brush _brush2;
        private Pen _pen2;
        private Brush RedBrush => _brush2 ?? (_brush2 = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 255, 150, 150)));
        private Pen RedPen => _pen2 ?? (_pen2 = Svg.Engine.Factory.CreatePen(RedBrush, 3));

        public bool IsDebugEnabled { get; set; }

        public Func<SvgVisualElement, bool> Filter { get; set; }
        
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
                    _lastRotationCenter = null;
                }
            }
            
            return Task.FromResult(true);
        }

        public override Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            if(IsDebugEnabled && _lastRotationCenter != null)
                renderer.DrawCircle(_lastRotationCenter.X, _lastRotationCenter.Y, 2, RedPen);

            return Task.FromResult(true);
        }

        private void RotateElement(SvgVisualElement element, RotateEvent rotateEvent, SvgDrawingCanvas ws)
        {
            // if element must not be rotated
            if (Filter?.Invoke(element) == false)
                return;

            var b = element.GetBoundingBox();
            var centerX = b.X + (b.Width / 2);
            var centerY = b.Y + (b.Height / 2);

            _lastRotationCenter = PointF.Create(centerX, centerY);

            var rotateTrans = element.Transforms.OfType<SvgMatrix>().LastOrDefault();

            // in case there is no SvgMatrix transformation present yet...
            if (rotateTrans == null)
            {
                // we create our own one
                var m = Matrix.Create();

                // to rotate precisely at the center
                // we need to undo all translates of the preceding transformations
                var matrix = element.Transforms.GetMatrix();
                centerX -= matrix.OffsetX;
                centerY -= matrix.OffsetY;



                // then apply the transformation
                m.RotateAt(rotateEvent.AbsoluteRotationDegrees, PointF.Create(centerX, centerY), MatrixOrder.Prepend);

                // and add it
                element.Transforms.Add(m.ToSvgMatrix());
            }
            else
            {
                var m = rotateTrans.Matrix;

                // to rotate precisely at the center
                // we need to undo all translates of the preceding transformations
                var matrix = element.Transforms.GetMatrix();
                centerX -= matrix.OffsetX;
                centerY -= matrix.OffsetY;

                // then apply the transformation
                m.RotateAt(rotateEvent.RelativeRotationDegrees, PointF.Create(centerX, centerY), MatrixOrder.Prepend);
            }

            ws.FireInvalidateCanvas();
        }

        public override void Dispose()
        {
            _brush2?.Dispose();
            _pen2?.Dispose();

            base.Dispose();
        }
    }
}
