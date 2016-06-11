using Svg.Interfaces;

namespace Svg
{
    public abstract class SvgPlatformSetupBase
    {
        private static readonly FileSystem FileSystem = new FileSystem();
        private static readonly SvgElementFactory ElementFactory = new SvgElementFactory();
        private static readonly SvgMarshal Marshal = new SvgMarshal();
        private static readonly SvgTypeDescriptor SvgTypeDescriptor = new SvgTypeDescriptor();
        private static readonly SvgCharComverter CharConverter = new SvgCharComverter();
        private static readonly SvgElementAttributeProvider SvgElementAttributeProvider = new SvgElementAttributeProvider();
        private static readonly DefaultLogger DefaultLogger = new DefaultLogger();

        protected virtual void Initialize()
        {
            Engine.Register<IMarshal, SvgMarshal>(() => Marshal);
            Engine.Register<ISvgTypeDescriptor, SvgTypeDescriptor>(() => SvgTypeDescriptor);
            Engine.Register<ISvgElementAttributeProvider, SvgElementAttributeProvider>(() => SvgElementAttributeProvider);
            Engine.Register<ILogger, DefaultLogger>(() => DefaultLogger);
            Engine.Register<ICharConverter, SvgCharComverter>(() => CharConverter);
            Engine.Register<IWebRequest, WebRequestSvc>(() => new WebRequestSvc());
            Engine.Register<IFileSystem, FileSystem>(() => FileSystem);
            Engine.Register<ISvgUnitConverter, SvgUnitConverter>(() => new SvgUnitConverter());
            Engine.Register<ISvgElementFactory, SvgElementFactory>(() => ElementFactory);

            // register enumconverters
            // see http://stackoverflow.com/questions/1999803/how-to-implement-a-typeconverter-for-a-type-and-property-i-dont-own
        }
    }
}