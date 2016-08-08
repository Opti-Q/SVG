using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core;
using Svg.Core.Events;
using Svg.Interfaces;

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
        
        protected async Task Rotate(params float[] relativeAnglesDegree)
        {
            await Canvas.OnEvent(new RotateEvent(0, 0, RotateStatus.Start));

            var sum = 0f;
            foreach (var a in relativeAnglesDegree)
            {
                sum += a;
                await Canvas.OnEvent(new RotateEvent(a, sum, RotateStatus.Rotating));
            }

            await Canvas.OnEvent(new RotateEvent(0, sum, RotateStatus.End));
        }

        protected async Task Move(PointF start, PointF end)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            var delta = end - start;
            await Canvas.OnEvent(new MoveEvent(start, start, end, delta));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, start, start, end, 1));
        }
    }
}
