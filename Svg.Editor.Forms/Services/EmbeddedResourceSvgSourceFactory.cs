using System.Reflection;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg.Editor.Forms.Services
{
    public class EmbeddedResourceSvgSourceFactory : ISvgSourceFactory
    {
        public ISvgSource Create(string path)
        {
            return new EmbeddedResourceSource(path, GetType().GetTypeInfo().Assembly);
        }
    }
}
