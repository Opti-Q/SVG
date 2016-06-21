using System.IO;

namespace Svg.Droid.SampleEditor.Core.Interfaces
{
    public interface IImageStorer
    {
        void SaveAsPng(Bitmap image, Stream stream);
    }
}
