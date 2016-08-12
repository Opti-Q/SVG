using System;
using System.Diagnostics;

namespace Svg.Core.Events
{
    public enum RotateStatus
    {
        Start,
        Rotating,
        End
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public class RotateEvent : UserInputEvent
    {

        public RotateEvent(float relativeRotationDegrees, float absoluteRotationDegrees, RotateStatus status)
        {
            RelativeRotationDegrees = relativeRotationDegrees;
            AbsoluteRotationDegrees = absoluteRotationDegrees;
            Status = status;
        }

        public float RelativeRotationDegrees { get; private set; }
        public float AbsoluteRotationDegrees { get; private set; }
        public RotateStatus Status { get; private set; }

        public override string DebuggerDisplay => $"Rotate '{Enum.GetName(typeof(RotateStatus), Status)}' relative delta {RelativeRotationDegrees}, absolute delta {AbsoluteRotationDegrees}";
    }
}
