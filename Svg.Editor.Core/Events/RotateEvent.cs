using System.Diagnostics;

namespace Svg.Core.Events
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class RotateEvent : UserInputEvent
    {
        public RotateEvent(float relativeRotationDegrees, float absoluteRotationDegrees)
        {
            RelativeRotationDegrees = relativeRotationDegrees;
            AbsoluteRotationDegrees = absoluteRotationDegrees;
        }

        public float RelativeRotationDegrees { get; private set; }
        public float AbsoluteRotationDegrees { get; private set; }
        
        public string DebuggerDisplay => $"Rotate angle: relative delta {RelativeRotationDegrees}, absolute delta {AbsoluteRotationDegrees}";
    }
}
