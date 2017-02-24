using Svg.Interfaces;

namespace Svg
{
    public abstract class SvgPlatformSetupBase
    {
        private static readonly FileSystem FileSystem = new FileSystem();
        private static readonly SvgMarshal Marshal = new SvgMarshal();
        private static readonly SvgCharComverter CharConverter = new SvgCharComverter();
        private static readonly SvgElementAttributeProvider SvgElementAttributeProvider = new SvgElementAttributeProvider();
        private static readonly DefaultLogger DefaultLogger = new DefaultLogger();
        private static readonly CultureHelper CultureHelper = new CultureHelper();
        private bool _isInitialized = false;

        public virtual void Initialize()
        {
            if (_isInitialized)
                return;

            Engine.Register<IMarshal, SvgMarshal>(() => Marshal);
            Engine.Register<ISvgElementAttributeProvider, SvgElementAttributeProvider>(() => SvgElementAttributeProvider);
            Engine.Register<ICultureHelper, CultureHelper>(() => CultureHelper);
            Engine.Register<ILogger, DefaultLogger>(() => DefaultLogger);
            Engine.Register<ICharConverter, SvgCharComverter>(() => CharConverter);
            Engine.Register<IWebRequest, WebRequestSvc>(() => new WebRequestSvc());
            Engine.Register<IFileSystem, FileSystem>(() => FileSystem);

            //// register enumconverters
            //// see http://stackoverflow.com/questions/1999803/how-to-implement-a-typeconverter-for-a-type-and-property-i-dont-own


            _isInitialized = true;
        }
    }
}