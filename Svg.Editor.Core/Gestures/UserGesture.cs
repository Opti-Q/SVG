namespace Svg.Core.Gestures
{
    public abstract class UserGesture
    {
        public abstract GestureType Type { get; }
    }

    public enum GestureType
    {
        Undefined, Tap, LongPress, Drag, DoubleTap
    }
}