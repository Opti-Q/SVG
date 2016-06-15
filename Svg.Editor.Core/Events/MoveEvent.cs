using System;
using System.Diagnostics;
using Svg.Interfaces;

namespace Svg.Core.Events
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class MoveEvent : PointerEvent
    {
        public PointF AbsoluteDelta { get; private set; }

        public MoveEvent(PointF pointer1DownPositon, PointF pointer1Position, PointF absoluteDelta)
            : base(EventType.Move, pointer1DownPositon, pointer1Position)
        {
            AbsoluteDelta = absoluteDelta;
        }

        public override string DebuggerDisplay => $"Move from x:{Pointer1DownPosition?.X} y:{Pointer1DownPosition?.Y} to x:{Pointer1Position?.X} y:{Pointer1Position?.Y}";
    }
}
