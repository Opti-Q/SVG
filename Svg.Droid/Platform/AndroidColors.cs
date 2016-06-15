using Svg.Interfaces;

namespace Svg.Platform
{
    public class AndroidColors : Colors
    {
        private Color _black;
        private Color _transparent;
        private Color _white;


        public Color Black => _black ?? (_black = new AndroidColor(0,0,0));
        public Color Transparent => _transparent ?? (_transparent = new AndroidColor(0, 0, 0, 0));
        public Color White => _white ?? (_white = new AndroidColor(255, 255, 255));
    }
}