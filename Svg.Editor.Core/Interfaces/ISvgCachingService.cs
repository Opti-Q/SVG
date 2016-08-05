using System;
using Svg.Interfaces;

namespace Svg.Core.Interfaces
{
    public interface ISvgCachingService
    {
        void SaveAsPng(string svgFilePath, string nameModifier, Action<SvgDocument> preprocessAction = null);
        string GetCachedPngPath(string svgFilePath, string nameModifier, IFileSystem fs);
    }
}