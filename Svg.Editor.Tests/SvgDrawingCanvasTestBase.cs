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
            await Canvas.OnEvent(new RotateEvent(0, 0, RotateStatus.Start, 2));

            var sum = 0f;
            foreach (var a in relativeAnglesDegree)
            {
                sum += a;
                await Canvas.OnEvent(new RotateEvent(a, sum, RotateStatus.Rotating, 2));
            }

            await Canvas.OnEvent(new RotateEvent(0, sum, RotateStatus.End, 2));
        }

        protected async Task Move(PointF start, PointF end)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            var delta = end - start;
            await Canvas.OnEvent(new MoveEvent(start, start, end, delta, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, start, start, end, 1));
        }
    }
}
