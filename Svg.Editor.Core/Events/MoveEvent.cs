using Svg.Interfaces;

namespace Svg.Core.Events
{
    public class MoveEvent : PointerEvent
    {
        public PointF AbsoluteDelta { get; private set; }

        public MoveEvent(PointF pointer1DownPositon, PointF pointer1Position, PointF absoluteDelta)
            : base(EventType.Move, pointer1DownPositon, pointer1Position)
        {
            AbsoluteDelta = absoluteDelta;
        }
    }
}
