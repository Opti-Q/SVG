using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class ColorToolTests : SvgDrawingCanvasTestBase
    {
        private MockColorInputService _colorMock;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _colorMock = new MockColorInputService();

            Engine.Register<IColorInputService, MockColorInputService>(() => _colorMock);
        }

        [Test]
        public async Task WhenUserCreatesText_FillAndStrokeHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var text = new SvgText("hello");
            colorTool.SelectedColor = Color.Create(colorTool.SelectableColors[2]);
            var color = colorTool.SelectedColor;

            // Preassert
            Assert.AreNotEqual(color, ((SvgColourServer) text.Fill)?.Colour);
            Assert.AreNotEqual(color, ((SvgColourServer) text.Stroke)?.Colour);

            // Act
            await Canvas.AddItemInScreenCenter(text);

            // Assert
            Assert.AreEqual(color, ((SvgColourServer) text.Fill)?.Colour);
            Assert.AreEqual(color, ((SvgColourServer) text.Stroke)?.Colour);
        }

        [Test]
        public async Task WhenUserCreatesRectangle_StrokeHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var rectangle = new SvgRectangle();

            // Act
            await Canvas.AddItemInScreenCenter(rectangle);

            // Assert
            var color = colorTool.SelectedColor;
            Assert.True(color.Equals(((SvgColourServer) rectangle.Stroke).Colour));
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
            await Canvas.AddItemInScreenCenter(text);

            // Assert
            Assert.True(colorTool.SelectedColor.Equals(((SvgColourServer) text.Stroke).Colour));
            Assert.True(colorTool.SelectedColor.Equals(((SvgColourServer) text.Fill).Colour));
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
            await Canvas.AddItemInScreenCenter(rectangle);

            // Assert
            Assert.True(colorTool.SelectedColor.Equals(((SvgColourServer) rectangle.Stroke).Colour));
        }

        [Test]
        public async Task WhenUserSelectsTextAndSelectsColor_StrokeAndFillHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var text = new SvgText("hello");
            await Canvas.AddItemInScreenCenter(text);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(text);
            changeColorCommand.Execute(null);

            // Assert
            Assert.True(color.Equals(((SvgColourServer) text.Stroke).Colour));
            Assert.True(color.Equals(((SvgColourServer) text.Fill).Colour));
        }

        [Test]
        public async Task WhenUserSelectsRectangleAndSelectsColor_StrokeHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var rectangle = new SvgRectangle();
            await Canvas.AddItemInScreenCenter(rectangle);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(rectangle);
            changeColorCommand.Execute(null);

            // Assert
            Assert.True(color.Equals(((SvgColourServer) rectangle.Stroke).Colour));
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
            await Canvas.AddItemInScreenCenter(parent);
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
            await Canvas.AddItemInScreenCenter(parent);
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
            await Canvas.AddItemInScreenCenter(parent);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act
            Canvas.SelectedElements.Add(parent);
            changeColorCommand.Execute(null);

            // Assert
            Assert.True(child.Stroke.Equals(parent.Stroke));
        }

        [Test]
        public async Task SetSelectableColorsButNoNamesInProperties_DefaultNamesWillBeUsed()
        {
            // Arrange

            var selectableColors = new[] {"#000000", "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF"};

            Engine.Register<ToolFactoryProvider, ToolFactoryProvider>(() => new ToolFactoryProvider(new Func<ITool>[]
            {
                () => new ColorTool(new Dictionary<string, object>
                {
                    { "selectablecolors", selectableColors }
                }, new UndoRedoService())
            }));

            Canvas = new SvgDrawingCanvas();
            await Canvas.EnsureInitialized();

            // Act

            var selectableColorNames = Canvas.Tools.OfType<ColorTool>().First().SelectableColorNames;

            // Assert

            Assert.AreEqual(selectableColors.Length, selectableColorNames.Length);
            Assert.True(selectableColors.All(s => selectableColorNames.Contains(s)));
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
