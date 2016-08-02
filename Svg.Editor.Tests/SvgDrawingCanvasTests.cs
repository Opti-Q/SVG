using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Tools;

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
            
            // Act
            Canvas.AddItemInScreenCenter(element);

            // Assert 
            var children = Canvas.Document.Children.OfType<SvgVisualElement>().ToList();
            Assert.AreEqual(1, children.Count);
            var child = children.Single();
            var b = child.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            Assert.AreEqual(355.793488f, b.X);
            Assert.AreEqual(220.0f, b.Y);
            Assert.AreEqual(69.2820435f, b.Width);
            Assert.GreaterOrEqual(b.Height, 51.2116699f);
        }
    }
}
