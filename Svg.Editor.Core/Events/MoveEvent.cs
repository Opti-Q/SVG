using System;
using System.Diagnostics;
using Svg.Interfaces;

namespace Svg.Core.Events
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class MoveEvent : PointerEvent
    {
        public PointF RelativeDelta { get; private set; }

        public MoveEvent(PointF pointer1Down, PointF lastPointer1Position, PointF pointer1Position, PointF relativeDelta)
            : base(EventType.Move, pointer1Down, lastPointer1Position, pointer1Position)
        {
            RelativeDelta = relativeDelta;
        }

        public override string DebuggerDisplay => $"Move from x:{LastPointer1DownPosition?.X} y:{LastPointer1DownPosition?.Y} to x:{Pointer1Position?.X} y:{Pointer1Position?.Y} (pointer down x:{Pointer1Down.X} y:{Pointer1Down.Y}";
    }
}
