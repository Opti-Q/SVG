using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Events;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class UndoRedoToolTests : SvgDrawingCanvasTestBase
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
        public async Task WhenUserCreatesText_FillAndStrokeHaveSelectedColor_ThenUndo_FillAndStrokeHaveOldColors()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var text = new SvgText("hello");
            colorTool.SelectedColor = Color.Create(colorTool.SelectableColors[2]);
            var color = colorTool.SelectedColor;
            var oldStroke = text.Stroke?.ToString();
            var oldFill = text.Fill?.ToString();
            Canvas.AddItemInScreenCenter(text);

            // Preassert
            Assert.AreEqual(color, ((SvgColourServer) text.Fill)?.Colour);
            Assert.AreEqual(color, ((SvgColourServer) text.Stroke)?.Colour);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(oldStroke, text.Stroke?.ToString());
            Assert.AreEqual(oldFill, text.Fill?.ToString());
        }

        [Test]
        public async Task WhenUserCreatesRectangle_StrokeHasSelectedColor_ThenUndo_StrokeHasOldColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var rectangle = new SvgRectangle();
            var oldStroke = rectangle.Stroke?.ToString();
            Canvas.AddItemInScreenCenter(rectangle);

            // Preassert
            var color = colorTool.SelectedColor;
            Assert.AreEqual(color, ((SvgColourServer) rectangle.Stroke)?.Colour);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(oldStroke, rectangle.Stroke?.ToString());
        }

        [Test]
        public async Task WhenUserSelectsTextAndSelectsColor_StrokeAndFillHaveSelectedColor_ThenUndo_StrokeAndFillHaveOldColors()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var text = new SvgText("hello");
            Canvas.AddItemInScreenCenter(text);
            var oldStroke = text.Stroke?.ToString();
            var oldFill = text.Fill?.ToString();
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();
            Canvas.SelectedElements.Add(text);
            changeColorCommand.Execute(null);

            // Pressert
            Assert.AreEqual(color, ((SvgColourServer) text.Stroke)?.Colour);
            Assert.AreEqual(color, ((SvgColourServer) text.Fill)?.Colour);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(oldStroke, text.Stroke?.ToString());
            Assert.AreEqual(oldFill, text.Fill?.ToString());
        }

        [Test]
        public async Task WhenUserSelectsRectangleAndSelectsColor_StrokeHasSelectedColor_ThenUndo_StrokeHasOldColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var rectangle = new SvgRectangle();
            Canvas.AddItemInScreenCenter(rectangle);
            var oldStroke = rectangle.Stroke?.ToString();
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();
            Canvas.SelectedElements.Add(rectangle);
            changeColorCommand.Execute(null);

            // Preassert
            Assert.True(color.Equals(((SvgColourServer) rectangle.Stroke)?.Colour));

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(oldStroke, rectangle.Stroke?.ToString());
        }

        private class MockColorInputService : IColorInputService
        {
            public Func<int> F { get; set; } = () => 0;

            public Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors)
            {
                return Task.FromResult(F());
            }
        }

        [Test]
        public async Task IfUserDrawsPath_PathIsCreated_ThenUndo_PathIsRemoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<FreeDrawingTool>().Single();
            Canvas.ActiveTool = tool;
            await Move(PointF.Create(10, 10), PointF.Create(10, 100));

            // Preassert
            Assert.AreEqual(1, Canvas.Document.Children.OfType<SvgPath>().Count());

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.IsEmpty(Canvas.Document.Children.OfType<SvgPath>());
        }

        [Test]
        public async Task IfUserDrawsLine_LineIsCreated_ThenUndo_LineIsRemoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            Canvas.ActiveTool = lineTool;
            await Move(PointF.Create(10, 10), PointF.Create(10, 100));

            // Preassert
            Assert.AreEqual(1, Canvas.Document.Descendants().OfType<SvgLine>().Count());

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.IsEmpty(Canvas.Document.Children.OfType<SvgLine>());
        }

        [Test]
        public async Task IfUserEditLine_LineSnapsToGrid_ThenUndo_LinePointsAreOnOldPositions()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            const float lineStartX = 0.0f;
            const float lineStartY = 0.0f;
            const float lineEndX = 34.64098f;
            const float lineEndY = 20.0f;
            var line = new SvgLine { StartX = lineStartX, StartY = lineStartY, EndX = lineEndX, EndY = lineEndY };
            Canvas.Document.Children.Add(line);
            Canvas.SelectedElements.Add(line);
            Canvas.ActiveTool = lineTool;
            await Move(PointF.Create(34.64098f, 20.0f), PointF.Create(70.0f, 40.0f));

            // Preassert
            var points = line.GetTransformedLinePoints();
            Assert.AreEqual(0.0f, points[0].X, 0.001f);
            Assert.AreEqual(0.0f, points[0].Y, 0.001f);
            Assert.AreEqual(69.28196f, points[1].X, 0.001f);
            Assert.AreEqual(40.0f, points[1].Y, 0.001f);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            points = line.GetTransformedLinePoints();
            Assert.AreEqual(lineStartX, points[0].X, 0.01f);
            Assert.AreEqual(lineStartY, points[0].Y, 0.01f);
            Assert.AreEqual(lineEndX, points[1].X, 0.01f);
            Assert.AreEqual(lineEndY, points[1].Y, 0.01f);
        }

        [Test]
        public async Task IfPointerIsMoved_And1ElementIsSelected_ElementIsMoved_ThenUndo_ElementIsMovedBack()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            var transforms = child.Transforms.Clone();
            var transforms1 = child1.Transforms.Clone();
            var transforms2 = child2.Transforms.Clone();
            await Move(PointF.Create(75, 75), PointF.Create(200, 100));

            // Preassert
            Assert.AreNotEqual(transforms, child.Transforms);
            Assert.AreEqual(transforms1, child1.Transforms);
            Assert.AreEqual(transforms2, child2.Transforms);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(transforms, child.Transforms);
        }

        [Test]
        public async Task IfPointerIsMoved_AndElementsAreSelected_ElementsAreMoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child1);
            Canvas.SelectedElements.Add(child2);
            var transforms = child.Transforms.Clone();
            var transforms1 = child1.Transforms.Clone();
            var transforms2 = child2.Transforms.Clone();
            await Move(PointF.Create(75, 75), PointF.Create(200, 100));

            // Preassert
            Assert.AreNotEqual(transforms, child.Transforms);
            Assert.AreNotEqual(transforms1, child1.Transforms);
            Assert.AreNotEqual(transforms2, child2.Transforms);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(transforms, child.Transforms);
            Assert.AreEqual(transforms1, child1.Transforms);
            Assert.AreEqual(transforms2, child2.Transforms);
        }

        [Test]
        public async Task SingleElementSelected_Rotate_RotatesElementUsingSvgMatrix_ThenUndo_RotatesElementBack()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 0),
                Y = new SvgUnit(SvgUnitType.Pixel, 0),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
            };
            Canvas.Document.Children.Add(element1);
            Canvas.SelectedElements.Add(element1);
            var matrix = element1.Transforms.GetMatrix();
            var formerMatrix = matrix.Clone();
            matrix.RotateAt(55f, PointF.Create(15, 10), MatrixOrder.Prepend);
            await Rotate(45, 10);

            // Preassert
            var actual = element1.Transforms.GetMatrix();
            AssertAreEqual(matrix, actual);
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.IsTrue(Canvas.SelectedElements.Single() == element1, "must still be selected");

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            actual = element1.Transforms.GetMatrix();
            AssertAreEqual(formerMatrix, actual);
            Assert.AreEqual(0, Canvas.SelectedElements.Count, "must be deselected");
        }

        private void AssertAreEqual(Matrix expected, Matrix actual)
        {
            Assert.AreEqual(expected.ScaleX, actual.ScaleX, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.ScaleY, actual.ScaleY, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.OffsetX, actual.OffsetX, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.OffsetY, actual.OffsetY, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.SkewX, actual.SkewX, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.SkewY, actual.SkewY, 0.05, $"{expected} but was \n{actual}");
        }

        [Test]
        public async Task ElementsAreSelected_AndDeleteCommandExecuted_ElementsAreRemoved_ThenUndo_ElementsAreAddedAgain()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var selectionTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = selectionTool;
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
            var d = LoadDocument("nested_transformed_text.svg");
            var element2 = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.AddItemInScreenCenter(element2);
            Canvas.SelectedElements.Add(element1);
            Canvas.SelectedElements.Add(element2);
            selectionTool.Commands.First().Execute(null);

            // Preassert
            Assert.False(Canvas.Document.Children.Any(x => x == element1));
            Assert.False(Canvas.Document.Children.Any(x => x == element2));
            Assert.False(Canvas.SelectedElements.Any());

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.Any(x => x == element1));
            Assert.True(Canvas.Document.Children.Any(x => x == element2));
            Assert.False(Canvas.SelectedElements.Any());
        }
    }
}