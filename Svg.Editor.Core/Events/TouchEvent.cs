using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Interfaces;

namespace Svg.Core.Events
{
    public enum EventType
    {
        PointerDown,
        PointerUp,
    }

    public class TouchEvent : UserInputEvent
    {
        public EventType EventType { get; private set; }

        public TouchEvent(EventType eventType, PointF pointer1DownPositon, PointF pointer1Position)
        {
            EventType = eventType;
            Pointer1DownPosition = pointer1DownPositon;
            Pointer1Position = pointer1Position;
        }

        public PointF Pointer1DownPosition { get; set; }

        public PointF Pointer1Position { get; set; }
    }
}
