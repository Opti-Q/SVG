using System;
using System.IO;
using Svg.Interfaces;

namespace Svg
{
    public class SaveAsPngOptions
    {
        private SizeF _imageDimension  =  null;
        public Action<SvgDocument> PreprocessAction { get; set; }
        public SizeF ImageDimension
        {
            get { return _imageDimension ?? SizeF.Create(120, 120); }
            set { _imageDimension = value; }
        }
        public Func<string, SaveAsPngOptions, string> NamingConvention { get; protected set; } = (key, op) => $"{Path.GetFileNameWithoutExtension(key).ToLower().Replace(".", "_")}_{(int) op.ImageDimension.Width}px_{(int) op.ImageDimension.Width}px{op.CustomPostFix?.Invoke(key, op)}.png";
        public Color BackgroundColor { get; set; } = Engine.Factory.Colors.Transparent;
        public bool Force { get; set; }
        public Func<string, SaveAsPngOptions, string> CustomPostFix { get; set; }
    }

    public interface ISvgCachingService
    {
        string GetCachedPng(string svgFilePath, SaveAsPngOptions options);
        void Clear();
    }
}