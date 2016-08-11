using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Events;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class LineToolTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public async Task IfUserTapsCanvas_AndDoesNotMove_NoLineIsDrawn()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            Canvas.ActiveTool = lineTool;

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
        public async Task IfUserDrawsLine_AndMovesTooLess_NoLineIsCreated()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            Canvas.ActiveTool = lineTool;

            // Act
            var start = PointF.Create(10, 10);
            var end = PointF.Create(10, 39);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.False(Canvas.Document.Descendants().OfType<SvgLine>().Any());
        }

        [Test]
        public async Task IfUserDrawsLine_LineIsCreated()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            Canvas.ActiveTool = lineTool;

            // Act
            var start = PointF.Create(10, 10);
            var end = PointF.Create(10, 100);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.AreEqual(1, Canvas.Document.Descendants().OfType<SvgLine>().Count());
        }
    }
}
