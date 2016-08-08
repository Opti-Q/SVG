using System;
using System.Linq;
using Svg.Interfaces;

namespace Svg.Core.Utils
{
    public static class SvgProcessingUtil
    {
        public static Action<SvgDocument> ColorAction(Color color)
        {
            return document =>
            {
                document.Children.Single().Children.Last().Fill = new SvgColourServer(color);
            };
        }
    }
}
