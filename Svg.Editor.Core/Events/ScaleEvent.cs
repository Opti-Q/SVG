
using System;
using System.Diagnostics;

namespace Svg.Core.Events
{
    public enum ScaleStatus
    {
        Start,
        Scaling,
        End
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public class ScaleEvent : UserInputEvent
    {
        public ScaleStatus Status { get; private set; }
        public float ScaleFactor { get; private set; }
        public float FocusX { get; private set; }
        public float FocusY { get; private set; }

        public ScaleEvent(ScaleStatus status, float scaleFactor, float focusX, float focusY)
        {
            Status = status;
            ScaleFactor = scaleFactor;
            FocusX = focusX;
            FocusY = focusY;
        }

        public override string DebuggerDisplay => $"Scale ({Enum.GetName(typeof(ScaleStatus), Status)}) {ScaleFactor} at x:{FocusX} y:{FocusY}";

        public override string ToString()
        {
            return DebuggerDisplay;
        }
    }
}
