using NUnit.Framework;
using Svg.Core.Tools;

namespace Svg.Editor.Core.Tests
{
    [TestFixture]
    public class SvgDrawingCanvasTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public void HasExplicitTool()
        {
            // Assert
            var t = Canvas.ActiveTool;

            Assert.IsNotNull(t);
            Assert.AreEqual(ToolUsage.Explicit, t.ToolUsage);
        }
    }
}
