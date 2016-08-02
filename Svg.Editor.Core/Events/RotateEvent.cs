using System.Diagnostics;

namespace Svg.Core.Events
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class RotateEvent : UserInputEvent
    {
        public RotateEvent(float angle)
        {
            Angle = angle;
        }

        public float Angle { get; private set; }
        

        public string DebuggerDisplay => $"Rotate angle: {Angle}";
    }
}
