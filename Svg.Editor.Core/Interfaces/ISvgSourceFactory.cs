using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core
{
    public interface ISvgSourceFactory
    {
        ISvgSource Create(string path);
    }
}
