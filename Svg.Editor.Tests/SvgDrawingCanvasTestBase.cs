using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Svg.Core;
using Svg.Core.Events;
using Svg.Core.Services;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    public abstract class SvgDrawingCanvasTestBase
    {
        private SvgDrawingCanvas _canvas;

        protected SvgDrawingCanvas Canvas => _canvas;
        protected SchedulerProvider SchedulerProvider { get; } = new SchedulerProvider(new TestScheduler(), new TestScheduler());

        [SetUp]
        public virtual void SetUp()
        {
            Engine.Register<SchedulerProvider, SchedulerProvider>(() => SchedulerProvider);

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
            await Canvas.OnEvent(new RotateEvent(0, 0, RotateStatus.Start, 3));

            var sum = 0f;
            foreach (var a in relativeAnglesDegree)
            {
                sum += a;
                await Canvas.OnEvent(new RotateEvent(a, sum, RotateStatus.Rotating, 3));
            }

            await Canvas.OnEvent(new RotateEvent(0, sum, RotateStatus.End, 0));
        }

        protected async Task Move(PointF start, PointF end, int pointerCount = 1)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, pointerCount));
            var delta = end - start;
            await Canvas.OnEvent(new MoveEvent(start, start, end, delta, pointerCount));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, start, start, end, pointerCount));
        }
    }
}
