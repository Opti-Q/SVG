using System.Threading.Tasks;

namespace Svg.Core.Interfaces
{
    public interface IPickImageService
    {
        Task<string> PickImagePath(int maxPixelDimension);
    }
}