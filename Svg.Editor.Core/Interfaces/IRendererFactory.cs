
namespace Svg.Core.Interfaces
{
    public interface IRendererFactory
    {
        IRenderer Create(Bitmap bitmap);
    }
}
