using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidColors : Colors
    {
        private Color _black;
        private AndroidColor _transparent;


        public Color Black => _black ?? (_black = new AndroidColor(0,0,0));
        public Color Transparent => _transparent ?? (_transparent = new AndroidColor(0, 0, 0, 0));
    }
}