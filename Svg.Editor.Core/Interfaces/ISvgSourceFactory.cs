using Svg.Interfaces;

namespace Svg.Core.Interfaces
{
    public interface ISvgSourceFactory
    {
        ISvgSource Create(string path);
    }
}
