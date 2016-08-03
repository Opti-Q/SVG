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
        public async Task WhenUserCreatesText_HasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();

            // Act
            Canvas.AddItemInScreenCenter(new SvgText("hello"));

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            var txt = texts.First();
            var color = colorTool.SelectedColor;
            Assert.AreEqual(color.A, ((SvgColourServer)txt.Fill).Colour.A);
            Assert.AreEqual(color.R, ((SvgColourServer)txt.Fill).Colour.R);
            Assert.AreEqual(color.G, ((SvgColourServer)txt.Fill).Colour.G);
            Assert.AreEqual(color.B, ((SvgColourServer)txt.Fill).Colour.B);
        }

        [Test]
        public async Task WhenUserSelectsColorAndCreatesText_HasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var color = Color.Create(0, 255, 0);

            // Act
            colorTool.SelectedColor = color;
            Canvas.AddItemInScreenCenter(new SvgText("hello"));

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            var txt = texts.First();
            Assert.AreEqual(color.A, ((SvgColourServer)txt.Stroke).Colour.A);
            Assert.AreEqual(color.R, ((SvgColourServer)txt.Stroke).Colour.R);
            Assert.AreEqual(color.G, ((SvgColourServer)txt.Stroke).Colour.G);
            Assert.AreEqual(color.B, ((SvgColourServer)txt.Stroke).Colour.B);
            Assert.AreEqual(color.A, ((SvgColourServer)txt.Fill).Colour.A);
            Assert.AreEqual(color.R, ((SvgColourServer)txt.Fill).Colour.R);
            Assert.AreEqual(color.G, ((SvgColourServer)txt.Fill).Colour.G);
            Assert.AreEqual(color.B, ((SvgColourServer)txt.Fill).Colour.B);
        }

        [Test]
        public async Task WhenUserSelectsTextAndSelectsColor_HasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1];
            _colorMock.F = () => 1;
            var element = new SvgText("hello");
            Canvas.AddItemInScreenCenter(element);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(element);
            changeColorCommand.Execute(null);

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            var txt = texts.First();
            Assert.AreEqual(color.A, ((SvgColourServer)txt.Stroke).Colour.A);
            Assert.AreEqual(color.R, ((SvgColourServer)txt.Stroke).Colour.R);
            Assert.AreEqual(color.G, ((SvgColourServer)txt.Stroke).Colour.G);
            Assert.AreEqual(color.B, ((SvgColourServer)txt.Stroke).Colour.B);
            Assert.AreEqual(color.A, ((SvgColourServer)txt.Fill).Colour.A);
            Assert.AreEqual(color.R, ((SvgColourServer)txt.Fill).Colour.R);
            Assert.AreEqual(color.G, ((SvgColourServer)txt.Fill).Colour.G);
            Assert.AreEqual(color.B, ((SvgColourServer)txt.Fill).Colour.B);
        }

        [Test]
        public async Task WhenUserSelectsParentAndSelectsColor_ChildHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1];
            _colorMock.F = () => 1;
            var parent = new SvgGroup();
            var child = new SvgText("hello");
            parent.Children.Add(child);
            Canvas.AddItemInScreenCenter(parent);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(parent);
            changeColorCommand.Execute(null);

            // Assert
            var texts = Canvas.Document.Children.First().Children.OfType<SvgTextBase>().ToList(); 
            var txt = texts.First();
            Assert.AreEqual(color.A, ((SvgColourServer)txt.Stroke).Colour.A);
            Assert.AreEqual(color.R, ((SvgColourServer)txt.Stroke).Colour.R);
            Assert.AreEqual(color.G, ((SvgColourServer)txt.Stroke).Colour.G);
            Assert.AreEqual(color.B, ((SvgColourServer)txt.Stroke).Colour.B);
            Assert.AreEqual(color.A, ((SvgColourServer)txt.Fill).Colour.A);
            Assert.AreEqual(color.R, ((SvgColourServer)txt.Fill).Colour.R);
            Assert.AreEqual(color.G, ((SvgColourServer)txt.Fill).Colour.G);
            Assert.AreEqual(color.B, ((SvgColourServer)txt.Fill).Colour.B);
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

            public Task<int> GetIndexFromUserInput(string title, string[] items)
            {
                return Task.FromResult(F());
            }
        }
    }
}
