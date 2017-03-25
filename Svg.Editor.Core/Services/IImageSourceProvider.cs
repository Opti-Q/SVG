using Svg.Interfaces;

namespace Svg.Editor.Services
{
    public interface IImageSourceProvider
    {
        string GetImage(string image, SizeF dimension = null);
    }
}