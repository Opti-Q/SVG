using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Events;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class SelectionToolTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public async Task CanSelectTransformedElement_ByTappingIt()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = txtTool;

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.AddItemInScreenCenter(child);
            // Preassert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);

            // Act
            var pt1 = PointF.Create(370, 260);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1));

            // Assert
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.AreSame(child, Canvas.SelectedElements.Single());
        }

        [Test]
        public async Task CanDeselect_ByTappingToNowhere()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = txtTool;

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.AddItemInScreenCenter(child);

            var pt1 = PointF.Create(370, 260);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1));

            // Act
            var pt2 = PointF.Create(0, 0);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt2, pt2, pt2));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt2, pt2, pt2));

            // Assert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task CanSelectTransformedElement_ByDrawingSelectionRectangle()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 200),
                Y = new SvgUnit(SvgUnitType.Pixel, 200),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
            };
            Canvas.Document.Children.Add(element1);
            var b1 = element1.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            var d = LoadDocument("nested_transformed_text.svg");
            var element2 = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.AddItemInScreenCenter(element2);
            var b2 = element2.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            // Preassert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);

            // Act
            var start = PointF.Create(b1.Left - 1, b1.Top - 1);
            var end = PointF.Create(b2.Right + 1, b2.Bottom + 1);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end));

            // Assert
            Assert.AreEqual(2, Canvas.SelectedElements.Count);
            Assert.AreSame(element2, Canvas.SelectedElements.First(), "z-index wrong?");
            Assert.AreSame(element1, Canvas.SelectedElements.Last(), "z-index wrong?");
        }
    }
}
