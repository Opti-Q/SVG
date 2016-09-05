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

            var m = ws.GetCanvasTransformationMatrix();
            var zoom = Matrix.Create(ws.ZoomFactor, 0, 0, ws.ZoomFactor, 0, 0);
            var translate = Matrix.Create(1, 0, 0, 1, ws.Translate.X, ws.Translate.Y);
            zoom.Invert();
            translate.Invert();
            zoom.Multiply(translate);
            m.Multiply(zoom);
            m.Invert();

            var topleft = PointF.Create(ConstraintLeft, ConstraintTop);
            var bottomright = PointF.Create(ConstraintRight, ConstraintBottom);
            m.TransformPoints(new[] { topleft, bottomright });

            var translateX = ws.Translate.X + ev.RelativeDelta.X;
            ws.Translate.X = translateX >= topleft.X ? topleft.X : translateX - ws.ScreenWidth / ws.ZoomFactor <= -bottomright.X ? Math.Min(0, -(bottomright.X - ws.ScreenWidth / ws.ZoomFactor)) : translateX;

            var translateY = ws.Translate.Y + ev.RelativeDelta.Y;
            ws.Translate.Y = translateY >= topleft.Y ? topleft.Y : translateY - ws.ScreenHeight / ws.ZoomFactor <= -bottomright.Y ? Math.Min(0, -(bottomright.Y - ws.ScreenHeight / ws.ZoomFactor)) : translateY;

            //var topLeft = ws.ScreenToCanvas(0 + ev.RelativeDelta.X, 0 + ev.RelativeDelta.Y);
            //var bottomRight = ws.ScreenToCanvas(ws.ScreenWidth + ev.RelativeDelta.X, ws.ScreenHeight + ev.RelativeDelta.Y);
            //if (topLeft.X >= ConstraintLeft) ws.Translate.X += ev.RelativeDelta.X;
            //if (topLeft.Y >= ConstraintTop) ws.Translate.Y += ev.RelativeDelta.Y;

            ws.FireInvalidateCanvas();

            return Task.FromResult(true);
        }

        public float ConstraintTop
        {
            get
            {
                object constraintTop;
                if (!Properties.TryGetValue("constrainttop", out constraintTop))
                    constraintTop = .0f;
                return Convert.ToSingle(constraintTop);
            }
            set { Properties["constrainttop"] = value; }
        }

        public float ConstraintLeft
        {
            get
            {
                object constraintLeft;
                if (!Properties.TryGetValue("constraintleft", out constraintLeft))
                    constraintLeft = .0f;
                return Convert.ToSingle(constraintLeft);
            }
            set { Properties["constraintleft"] = value; }
        }

        public float ConstraintRight
        {
            get
            {
                object constraintRight;
                if (!Properties.TryGetValue("constraintright", out constraintRight))
                    constraintRight = 800.0f;
                return Convert.ToSingle(constraintRight);
            }
            set { Properties["constraintright"] = value; }
        }

        public float ConstraintBottom
        {
            get
            {
                object constraintBottom;
                if (!Properties.TryGetValue("constraintbottom", out constraintBottom))
                    constraintBottom = 600.0f;
                return Convert.ToSingle(constraintBottom);
            }
            set { Properties["constraintbottom"] = value; }
        }
    }
}
