using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class StrokeStyleToolTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public async Task StrokeStyleCommandExecuted_AllChildStrokesAreChanged()
        {
            // Arrange
            var tool = Canvas.Tools.OfType<StrokeStyleTool>().Single();
            var rect = new SvgRectangle
            {
                X = 10,
                Y = 10,
                Width = 50,
                Height = 50,
                Stroke = new SvgColourServer(Color.Create(0, 0, 0))
            };
            await Canvas.EnsureInitialized();

            Canvas.Document.Children.Add(rect);
            Canvas.SelectedElements.Add(rect);

            // Act
            tool.Commands.First().Execute(null);

            // Assert
            Assert.AreEqual("10 10", rect.StrokeDashArray.ToString());
        }
    }
}
