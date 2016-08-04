using System;
using Svg.Interfaces;

namespace Svg.Core.Interfaces
{
    public interface ISvgCachingService
    {
        void SaveAsPng(string sourceName, string name, Action<SvgDocument> preprocessAction = null);
    }
}