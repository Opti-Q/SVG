using System.IO;

namespace Svg.Core.Interfaces
{
    public interface IImageStorer
    {
        void SaveAsPng(Bitmap image, Stream stream);
    }
}
