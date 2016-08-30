using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Events;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class MoveToolTests : SvgDrawingCanvasTestBase
    {

        [Test]
        public async Task IfPointerIsMoved_AndNoElementIsSelected_NothingIsMoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().First(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            var transforms = child.Transforms.Clone();

            // Preassert
            Assert.AreEqual(transforms, child.Transforms);

            // Act
            var start = PointF.Create(100, 200);
            var end = PointF.Create(200, 100);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.AreEqual(transforms, child.Transforms);
        }

        [Test]
        public async Task IfPointerIsMoved_And1ElementIsSelected_ElementIsMoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle {X = 50, Y = 50, Width = 50, Height = 50};
            var child1 = new SvgRectangle {X = 250, Y = 150, Width = 100, Height = 25};
            var child2 = new SvgRectangle {X = 150, Y = 250, Width = 150, Height = 150};
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            var transforms = child.Transforms.Clone();

            // Preassert
            Assert.AreEqual(transforms, child.Transforms);

            // Act
            var start = PointF.Create(75, 75);
            var end = PointF.Create(200, 100);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.AreNotEqual(transforms, child.Transforms);
        }

    }
}
