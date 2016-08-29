using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Events;
using Svg.Core.Tools;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class FreeDrawingToolTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public async Task IfUserTapsCanvas_AndDoesNotMove_NoPathIsDrawn()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<FreeDrawingTool>().Single();
            Canvas.ActiveTool = tool;

            // Preassert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);

            // Act
            var pt1 = PointF.Create(10, 10);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));

            // Assert
            Assert.False(Canvas.Document.Descendants().OfType<SvgLine>().Any());
        }

        [Test]
        public async Task IfUserDrawsPath_AndMovesTooLess_NoPathIsCreated()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<FreeDrawingTool>().Single();
            Canvas.ActiveTool = tool;

            // Act
            var start = PointF.Create(10, 10);
            var end = PointF.Create(10, 20);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.False(Canvas.Document.Children.OfType<SvgPath>().Any());
        }

        [Test]
        public async Task IfUserDrawsPath_PathIsCreated()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<FreeDrawingTool>().Single();
            Canvas.ActiveTool = tool;

            // Act
            var start = PointF.Create(10, 10);
            var end = PointF.Create(10, 100);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.AreEqual(1, Canvas.Document.Children.OfType<SvgPath>().Count());
        }
    }
}
