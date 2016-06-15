using System;
using System.Diagnostics;
using Svg.Interfaces;

namespace Svg.Core.Events
{
    public enum EventType
    {
        PointerDown,
        Move,
        PointerUp,
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
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


        public virtual string DebuggerDisplay => $"Pointer ({Enum.GetName(typeof(EventType), EventType)}) from x:{Pointer1DownPosition?.X} y:{Pointer1DownPosition?.Y} to x:{Pointer1Position?.X} y:{Pointer1Position?.Y}";

        public override string ToString()
        {
            return DebuggerDisplay;
        }
    }
}
