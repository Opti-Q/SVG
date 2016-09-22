using Svg.Interfaces;

namespace Svg.Core.Gestures
{
    public class TapGesture : UserGesture
    {
        public override GestureType Type => GestureType.Tap;

        public PointF Position { get; }

        public TapGesture(PointF position)
        {
            Position = position;
        }
    }
}
