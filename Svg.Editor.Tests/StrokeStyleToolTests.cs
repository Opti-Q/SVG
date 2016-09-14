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
        public async Task OneElementSelected_StrokeStyleCommandExecuted_ChildStrokeIsChanged()
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
            Assert.AreEqual("3 3", rect.StrokeDashArray.ToString());
        }

        [Test]
        public async Task ManyElementsSelected_StrokeStyleCommandExecuted_AllChildStrokesAreChanged()
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
            var rect1 = new SvgRectangle
            {
                X = 100,
                Y = 100,
                Width = 50,
                Height = 50,
                Stroke = new SvgColourServer(Color.Create(0, 0, 0))
            };
            var rect2 = new SvgRectangle
            {
                X = 50,
                Y = 50,
                Width = 25,
                Height = 25,
                Stroke = new SvgColourServer(Color.Create(0, 0, 0))
            };
            await Canvas.EnsureInitialized();

            Canvas.Document.Children.Add(rect);
            Canvas.Document.Children.Add(rect1);
            Canvas.Document.Children.Add(rect2);
            Canvas.SelectedElements.Add(rect);
            Canvas.SelectedElements.Add(rect1);
            Canvas.SelectedElements.Add(rect2);

            // Act
            tool.Commands.First().Execute(null);

            // Assert
            Assert.AreEqual("3 3", rect.StrokeDashArray.ToString());
            Assert.AreEqual("3 3", rect1.StrokeDashArray.ToString());
            Assert.AreEqual("3 3", rect2.StrokeDashArray.ToString());
        }
    }
}
