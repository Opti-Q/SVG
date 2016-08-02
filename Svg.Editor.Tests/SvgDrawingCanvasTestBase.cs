using NUnit.Framework;
using Svg.Core;

namespace Svg.Editor.Tests
{
    public abstract class SvgDrawingCanvasTestBase
    {
        private SvgDrawingCanvas _canvas;

        protected SvgDrawingCanvas Canvas => _canvas;

        static SvgDrawingCanvasTestBase()
        {
            SvgPlatformSetup.Init(new SvgSkiaPlatformOptions() { EnableFastTextRendering = true });
#if WIN
            // register dummy factory for windows builds (only used in unittests)
            Engine.Register<IFactory, WinSKFactory>(() => new WinSKFactory());
            Engine.Register<IFileLoader, FileLoader>(() => new FileLoader());
#endif
        }

        [SetUp]
        public virtual void SetUp()
        {
            _canvas = new SvgDrawingCanvas();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _canvas.Dispose();
        }

        protected SvgDocument LoadDocument(string fileName)
        {
            var l = Engine.Resolve<IFileLoader>();
            return l.Load(fileName);
        }
    }
}
