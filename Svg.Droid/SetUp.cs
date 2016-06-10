using Svg.Interfaces;

namespace Svg
{
    public class SetUp
    {
        private static readonly FileSystem FileSystem = new FileSystem();
        private static readonly SvgElementFactory ElementFactory = new SvgElementFactory();

        public static void Initialize()
        {
            SvgSetup.Register<IFactory, Factory>(() => new Factory());

            SvgSetup.Register<IFileSystem, FileSystem>(() => FileSystem);
            SvgSetup.Register<ISvgUnitConverter, SvgUnitConverter>(()=> new SvgUnitConverter());
            SvgSetup.Register<ISvgElementFactory, SvgElementFactory>(() => ElementFactory);

            // register enumconverters
            // see http://stackoverflow.com/questions/1999803/how-to-implement-a-typeconverter-for-a-type-and-property-i-dont-own
        }
    }
}