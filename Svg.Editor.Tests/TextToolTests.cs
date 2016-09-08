using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Core.Events;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class TextToolTests : SvgDrawingCanvasTestBase
    {
        private MockTextInputService _textMock;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _textMock = new MockTextInputService();

            Engine.Register<ITextInputService, MockTextInputService>(() => _textMock);
        }

        [Test]
        public async Task WhenUserTapsToBlankArea_CreatesText()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            _textMock.F = (x, y) => new TextTool.TextProperties { Text = "hello", FontSizeIndex = 0 };

            // Act
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            var txt = texts.First();
            Assert.AreEqual("hello", txt.Text);
            Assert.AreEqual(txtTool.FontSizes.First(), txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);
        }

        [Test]
        public async Task WhenUserTapsOnExistingText_EditsText()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.Document.Children.Add(new SvgText()
            {
                Text = "this is a test",
                X = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                Y = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                FontSize = new SvgUnit(SvgUnitType.Pixel, 20),
            });

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = "hello", FontSizeIndex = 0 };

            // Act
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            var txt = texts.First();
            Assert.AreEqual("hello", txt.Text);
            Assert.AreEqual(txtTool.FontSizes.First(), txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);
        }

        [Test]
        public async Task WhenUserTapsOnExistingText_AndOnlyChangesFontSize_EditsFontSize()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.Document.Children.Add(new SvgText()
            {
                Text = "hello",
                X = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                Y = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                FontSize = new SvgUnit(SvgUnitType.Pixel, 20),
            });

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = "hello", FontSizeIndex = 0 };

            // Act
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            var txt = texts.First();
            Assert.AreEqual("hello", txt.Text);
            Assert.AreEqual(txtTool.FontSizes.First(), txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase((string) null)]
        public async Task WhenUserTapsOnExistingText_AndEntersEmpty_RemovesText(string theText)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.Document.Children.Add(new SvgText()
            {
                Text = "this is a test",
                X = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                Y = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                FontSize = new SvgUnit(SvgUnitType.Pixel, 20),
            });

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = theText };

            // Act
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(0, texts.Count);
        }

        [Test]
        [TestCase("", "  ")]
        [TestCase(" ", "  ")]
        [TestCase("\t", "  ")]
        [TestCase(null, "  ")]
        [TestCase("hello from svg", "hello from svg")]
        public async Task WhenUserTapsNestedTextSpan_EditsTextSpan(string theText, string expectedText)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.AddItemInScreenCenter(child);

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = theText };

            // Act
            var pt1 = PointF.Create(370, 260);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(0, texts.Count);

            var nestedText = Canvas.Document.Descendants().OfType<SvgTextSpan>().Single();
            Assert.AreEqual(expectedText, nestedText.Text);
        }

        private class MockTextInputService : ITextInputService
        {
            public Func<string, string, TextTool.TextProperties> F { get; set; } = (x, y) => null;

            public Task<TextTool.TextProperties> GetUserInput(string title, string textValue, IEnumerable<string> textSizeOptions, int textSizeSelected)
            {
                return Task.FromResult(F(title, textValue));
            }
        }
    }
}
