using Svg.Interfaces;

namespace Svg.Editor.Interfaces
{
    public interface ISvgSourceFactory
    {
        ISvgSource Create(string path);
    }
}
