using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class ColorToolTests : SvgDrawingCanvasTestBase
    {
        private MockTextInputService _textMock;
        private MockColorInputService _colorMock;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _textMock = new MockTextInputService();
            _colorMock = new MockColorInputService();

            Engine.Register<ITextInputService, MockTextInputService>(() => _textMock);
            Engine.Register<IColorInputService, MockColorInputService>(() => _colorMock);
        }

        [Test]
        public async Task WhenUserCreatesText_FillAndStrokeHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var text = new SvgText("hello");

            // Act
            Canvas.AddItemInScreenCenter(text);

            // Assert
            var color = colorTool.SelectedColor;
            Assert.True(color.Equals(((SvgColourServer)text.Fill).Colour));
            Assert.True(color.Equals(((SvgColourServer)text.Stroke).Colour));
        }

        [Test]
        public async Task WhenUserCreatesRectangle_StrokeHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var rectangle = new SvgRectangle();

            // Act
            Canvas.AddItemInScreenCenter(rectangle);

            // Assert
            var color = colorTool.SelectedColor;
            Assert.True(color.Equals(((SvgColourServer)rectangle.Stroke).Colour));
        }

        [Test]
        public async Task WhenUserSelectsColorAndCreatesText_StrokeAndFillHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var color = Color.Create(0, 255, 0);
            var text = new SvgText("hello");

            // Act
            colorTool.SelectedColor = color;
            Canvas.AddItemInScreenCenter(text);

            // Assert
            Assert.True(colorTool.SelectedColor.Equals(((SvgColourServer)text.Stroke).Colour));
            Assert.True(colorTool.SelectedColor.Equals(((SvgColourServer)text.Fill).Colour));
        }

        [Test]
        public async Task WhenUserSelectsColorAndCreatesRectangle_StrokeHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var color = Color.Create(0, 255, 0);
            var rectangle = new SvgRectangle();

            // Act
            colorTool.SelectedColor = color;
            Canvas.AddItemInScreenCenter(rectangle);

            // Assert
            Assert.True(colorTool.SelectedColor.Equals(((SvgColourServer)rectangle.Stroke).Colour));
        }

        [Test]
        public async Task WhenUserSelectsTextAndSelectsColor_StrokeAndFillHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var text = new SvgText("hello");
            Canvas.AddItemInScreenCenter(text);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(text);
            changeColorCommand.Execute(null);

            // Assert
            Assert.True(color.Equals(((SvgColourServer)text.Stroke).Colour));
            Assert.True(color.Equals(((SvgColourServer)text.Fill).Colour));
        }

        [Test]
        public async Task WhenUserSelectsRectangleAndSelectsColor_StrokeHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var rectangle = new SvgRectangle();
            Canvas.AddItemInScreenCenter(rectangle);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(rectangle);
            changeColorCommand.Execute(null);

            // Assert
            Assert.True(color.Equals(((SvgColourServer)rectangle.Stroke).Colour));
        }

        [Test]
        public async Task ChildStrokeSetToNone_WhenUserSelectsParentAndSelectsColor_ChildStrokeIsStillNone()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            _colorMock.F = () => 1;
            var parent = new SvgGroup();
            var child = new SvgRectangle { X = 10, Y = 10, Width = 60, Height = 40, Stroke = SvgPaintServer.None };
            parent.Children.Add(child);
            Canvas.AddItemInScreenCenter(parent);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(parent);
            changeColorCommand.Execute(null);

            // Assert
            Assert.True(child.Stroke == SvgPaintServer.None);
        }

        [Test]
        public async Task ChildStrokeNotSet_WhenUserSelectsParentAndSelectsColor_ChildStrokeEqualsParentStroke()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            _colorMock.F = () => 1;
            var parent = new SvgGroup();
            var child = new SvgRectangle { X = 10, Y = 10, Width = 60, Height = 40, Stroke = SvgColourServer.NotSet };
            parent.Children.Add(child);
            Canvas.AddItemInScreenCenter(parent);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(parent);
            changeColorCommand.Execute(null);

            // Assert
            Assert.True(child.Stroke.Equals(parent.Stroke));
        }

        [Test]
        public async Task ChildStrokeInherit_WhenUserSelectsParentAndSelectsColor_ChildStrokeEqualsParentStroke()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            _colorMock.F = () => 1;
            var parent = new SvgGroup();
            var child = new SvgRectangle { X = 10, Y = 10, Width = 60, Height = 40, Stroke = SvgColourServer.Inherit };
            parent.Children.Add(child);
            Canvas.AddItemInScreenCenter(parent);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(parent);
            changeColorCommand.Execute(null);

            // Assert
            Assert.True(child.Stroke.Equals(parent.Stroke));
        }

        private class MockTextInputService : ITextInputService
        {
            public Func<string, string, string> F { get; set; } = (x, y) => null;

            public Task<string> GetUserInput(string title, string textValue)
            {
                return Task.FromResult(F(title, textValue));
            }
        }

        private class MockColorInputService : IColorInputService
        {
            public Func<int> F { get; set; } = () => 0;

            public Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors)
            {
                return Task.FromResult(F());
            }
        }
    }
}
