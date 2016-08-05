using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class SvgDrawingCanvasTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public async Task HasOneActiveExplicitTool()
        {
            // Act
            await Canvas.EnsureInitialized();

            // Assert
            var t = Canvas.ActiveTool;

            Assert.IsNotNull(t);
            Assert.AreEqual(ToolUsage.Explicit, t.ToolUsage);

            var activeTools = Canvas.Tools.Where(to => to.IsActive && to.ToolUsage == ToolUsage.Explicit).ToList();
            Assert.AreEqual(1, activeTools.Count);
            Assert.AreSame(t, activeTools.Single());
        }

        [Test]
        public void SvgDocumentIsNeverNull()
        {
            Assert.IsNotNull(Canvas.Document);
        }

        [Test]
        public async Task CanChangeActiveTool()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
        
            // Act
            Canvas.ActiveTool = txtTool;

            // Assert
            Assert.IsTrue(txtTool.IsActive);

            var activeTools = Canvas.Tools.Where(to => to.IsActive && to.ToolUsage == ToolUsage.Explicit).ToList();
            Assert.AreEqual(1, activeTools.Count);
            Assert.AreSame(txtTool, activeTools.Single());
        }

        [Test]
        public async Task CanAddElementAtScreenCenter()
        {
            // Arrange
            await Canvas.EnsureInitialized();

            var d = LoadDocument("nested_transformed_text.svg");
            var element = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            var gt = Canvas.Tools.OfType<GridTool>().Single();
            gt.IsSnappingEnabled = false; // disable snapping in this case
            
            // Act
            Canvas.AddItemInScreenCenter(element);

            // Assert 
            var children = Canvas.Document.Children.OfType<SvgVisualElement>().ToList();
            Assert.AreEqual(1, children.Count);
            var child = children.Single();
            var b = child.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            var e = RectangleF.Create(365.359f, 224.3942f, 69.28204f, 51.21167f);
            EnsureRectanglesEqual(e, b);
        }

        private void EnsureRectanglesEqual(RectangleF expected, RectangleF actual)
        {
            Assert.AreEqual(Math.Round(expected.X, 2), Math.Round(actual.X, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Y, 2), Math.Round(actual.Y, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Width, 2), Math.Round(actual.Width, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Height, 2), Math.Round(actual.Height, 2), $"{expected} \nvs {actual}");
        }
    }
}
