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

            // for getting the translation from focal point zoom, take the canvas transformation
            // and remove zoom and translate from it.
            var m = ws.GetCanvasTransformationMatrix();
            var zoom = Matrix.Create(ws.ZoomFactor, 0, 0, ws.ZoomFactor, 0, 0);
            zoom.Invert();
            var translate = Matrix.Create(1, 0, 0, 1, ws.Translate.X, ws.Translate.Y);
            translate.Invert();
            zoom.Multiply(translate);
            m.Multiply(zoom);
            m.Invert();

            // the constraints are divided into 2 points, which relate to the maximum and minimum
            // translations that the canvas can have.
            PointF bottomright;
            PointF topleft;
            // if we are in portrait mode, we need to switch the constraints a bit.
            //if (ws.ScreenWidth < ws.ScreenHeight)
            //{
            //    bottomright = -PointF.Create(ConstraintBottom, ConstraintRight) * ws.ZoomFactor;
            //    topleft = -PointF.Create(ConstraintTop, ConstraintLeft) * ws.ZoomFactor;
            //}
            //else
            {
                bottomright = -PointF.Create(ConstraintRight, ConstraintBottom)*ws.ZoomFactor;
                topleft = -PointF.Create(ConstraintLeft, ConstraintTop)*ws.ZoomFactor;
            }
            m.TransformPoints(new[] { topleft, bottomright });

            // apply constraints to the pending horizontal pan
            var translateX = ws.Translate.X + ev.RelativeDelta.X;
            ws.Translate.X = translateX >= topleft.X ? topleft.X : translateX - ws.ScreenWidth <= bottomright.X ? Math.Min(0, bottomright.X + ws.ScreenWidth) : translateX;

            // apply constraints to the pending vertical pan
            var translateY = ws.Translate.Y + ev.RelativeDelta.Y;
            ws.Translate.Y = translateY >= topleft.Y ? topleft.Y : translateY - ws.ScreenHeight <= bottomright.Y ? Math.Min(0, bottomright.Y + ws.ScreenHeight) : translateY;

            ws.FireInvalidateCanvas();

            return Task.FromResult(true);
        }

        public float ConstraintTop
        {
            get
            {
                object constraintTop;
                if (!Properties.TryGetValue("constrainttop", out constraintTop))
                    constraintTop = float.MinValue;
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
                    constraintLeft = float.MinValue;
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
                    constraintRight = float.MaxValue;
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
                    constraintBottom = float.MaxValue;
                return Convert.ToSingle(constraintBottom);
            }
            set { Properties["constraintbottom"] = value; }
        }
    }
}
