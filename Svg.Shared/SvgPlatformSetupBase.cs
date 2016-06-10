using System.IO;
using Svg.Interfaces;

namespace Svg
{
    public abstract class SvgPlatformSetupBase
    {
        private static readonly FileSystem FileSystem = new FileSystem();
        private static readonly SvgElementFactory ElementFactory = new SvgElementFactory();
        private static readonly SvgMarshal Marshal = new SvgMarshal();
        private static readonly CharConverter CharConverter = new CharConverter();

        protected virtual void Initialize()
        {
            Engine.Register<IMarshal, SvgMarshal>(() => Marshal);
            Engine.Register<ICharConverter, CharConverter>(() => CharConverter);
            Engine.Register<IWebRequest, WebRequestSvc>(() => new WebRequestSvc());
            Engine.Register<IFileSystem, FileSystem>(() => FileSystem);
            Engine.Register<ISvgUnitConverter, SvgUnitConverter>(() => new SvgUnitConverter());
            Engine.Register<ISvgElementFactory, SvgElementFactory>(() => ElementFactory);

            // register enumconverters
            // see http://stackoverflow.com/questions/1999803/how-to-implement-a-typeconverter-for-a-type-and-property-i-dont-own
        }
    }
}