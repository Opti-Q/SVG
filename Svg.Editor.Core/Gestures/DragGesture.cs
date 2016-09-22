using Svg.Interfaces;

namespace Svg.Core.Gestures
{
    public class DragGesture : UserGesture
    {
        public static readonly DragGesture Exit = new DragGesture(PointF.Empty, PointF.Empty, SizeF.Empty, 0);

        public PointF Position { get; }
        public PointF Start { get; }
        public SizeF Delta { get; }
        public double Dist { get; }

        public override GestureType Type => GestureType.Drag;

        public DragGesture(PointF position, PointF start, SizeF delta, double dist)
        {
            Position = position;
            Start = start;
            Delta = delta;
            Dist = dist;
        }
    }
}