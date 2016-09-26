using Svg.Interfaces;

namespace Svg.Core.Gestures
{
    public class DoubleTapGesture : TapGesture
    {
        public override GestureType Type => GestureType.DoubleTap;

        public DoubleTapGesture(PointF position) : base(position)
        {
        }
    }
}
