using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Core.UndoRedo;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public class RotationTool : UndoableToolBase
    {

        #region Private fields

        private bool _wasImplicitlyActivated;
        private PointF _lastRotationCenter;
        private Brush _brush2;
        private Pen _pen2;
        private Brush RedBrush => _brush2 ?? (_brush2 = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 255, 150, 150)));
        private Pen RedPen => _pen2 ?? (_pen2 = Engine.Factory.CreatePen(RedBrush, 3));
        private readonly Dictionary<SvgElement, float> _rotations = new Dictionary<SvgElement, float>();
        private ITool _activatedFrom;

        #endregion

        #region Public properties

        public bool IsDebugEnabled { get; set; }

        public static readonly string FilterKey = @"filter";
        public static readonly string RotationStepKey = @"rotationstep";

        public Func<SvgVisualElement, bool> Filter
        {
            get
            {
                object filter;
                Properties.TryGetValue(FilterKey, out filter);
                return (Func<SvgVisualElement, bool>) filter;
            }
            set { Properties[FilterKey] = value; }
        }

        public float RotationStep
        {
            get
            {
                object rotationStep;
                Properties.TryGetValue(RotationStepKey, out rotationStep);
                return Convert.ToSingle(rotationStep);
            }
            set { Properties[RotationStepKey] = value; }
        }

        #endregion

        public RotationTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Rotate", properties, undoRedoService)
        {
            ToolType = ToolType.Modify;
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
                    ws.ActiveTool.ToolType == ToolType.Select &&
                    // and the gesture is made with 3 fingers
                    re.PointerCount == 3)
                {
                    // implicitly activate
                    _activatedFrom = ws.ActiveTool;
                    ws.ActiveTool = this;
                    _wasImplicitlyActivated = true;
                    zt.IsActive = false;
                    _rotations.Clear();

                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Rotate operation", o => { }));
                }
                else if (re.Status == RotateStatus.Rotating &&
                         ws.SelectedElements.Count == 1 &&
                         re.PointerCount == 3)
                {
                    RotateElement(ws.SelectedElements[0], re, ws);
                }
                else if (re.Status == RotateStatus.End)
                {
                    if (ws.ActiveTool == this && _wasImplicitlyActivated)
                    {
                        ws.ActiveTool = _activatedFrom;
                    }
                    zt.IsActive = true;
                    _lastRotationCenter = null;
                    _rotations.Clear();
                }
            }

            return Task.FromResult(true);
        }

        public override Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            if (IsDebugEnabled && _lastRotationCenter != null)
                renderer.DrawCircle(_lastRotationCenter.X, _lastRotationCenter.Y, 2, RedPen);

            return Task.FromResult(true);
        }

        private void RotateElement(SvgVisualElement element, RotateEvent rotateEvent, SvgDrawingCanvas ws)
        {
            // if element must not be rotated
            if (Filter?.Invoke(element) == false)
                return;

            // always rotate by absolute radius!
            float previousAngle;
            if (!_rotations.TryGetValue(element, out previousAngle))
            {
                previousAngle = 0f;
            }

            var absoluteAngle = rotateEvent.AbsoluteRotationDegrees;
            var angle = CalculateNewRotation(absoluteAngle);
            var delta = angle - previousAngle;

            _rotations[element] = angle;

            if (Math.Abs(delta) > 0.01f)
            {
                var formerM = element.Transforms.GetMatrix().Clone();
                var m = element.CreateOriginRotation(delta % 360);

                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Rotate element", o =>
                {
                    element.SetTransformationMatrix(m);
                    ws.FireInvalidateCanvas();
                }, o =>
                {
                    element.SetTransformationMatrix(formerM);
                    ws.FireInvalidateCanvas();
                }), hasOwnUndoRedoScope: false);

            }
        }

        private float CalculateNewRotation(float absoluteAngle)
        {
            // if we can rotate with any angle, just return the absolute one
            if (RotationStep <= 0)
                return absoluteAngle;

            // else make sure we only rotate with the specified step size (e.g. 45°)
            var rest = absoluteAngle % RotationStep;

            // if the remainder is less than half the step size, just remove it
            if (rest <= RotationStep / 2)
            {
                return absoluteAngle - rest;
            }
            // otherwise round up to the next allowed angle (add stepsize)
            return absoluteAngle - rest + RotationStep;
        }

        public override void Dispose()
        {
            _brush2?.Dispose();
            _pen2?.Dispose();

            base.Dispose();
        }
    }
}
