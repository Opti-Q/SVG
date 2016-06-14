using Svg.Interfaces;

namespace Svg.Core.Events
{
    public enum EventType
    {
        PointerDown,
        Move,
        PointerUp,
    }

    public class PointerEvent : UserInputEvent
    {
        public EventType EventType { get; private set; }

        public PointerEvent(EventType eventType, PointF pointer1DownPositon, PointF pointer1Position)
        {
            EventType = eventType;
            Pointer1DownPosition = pointer1DownPositon;
            Pointer1Position = pointer1Position;
        }

        public PointF Pointer1DownPosition { get; set; }

        public PointF Pointer1Position { get; set; }
    }
}
