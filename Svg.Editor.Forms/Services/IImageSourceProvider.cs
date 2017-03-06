using Xamarin.Forms;

namespace Svg.Editor.Forms.Services
{
    public interface IImageSourceProvider
    {
        FileImageSource GetImage(string image);
    }
}