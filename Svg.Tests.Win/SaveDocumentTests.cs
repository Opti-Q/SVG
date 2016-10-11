using System.IO;
using System.Linq;
using NUnit.Framework;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class SaveDocumentTests
    {
        [SetUp]
        public void SetUp()
        {
            var setup = new SvgPlatformSetup();
        }

        [Test]
        public void SavingDocument_KeepsInheritedAttributesIntact()
        {
            // Arrange
            var doc = new SvgDocument()
            {
                Children =
                {
                    new SvgGroup()
                    {
                        Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                        StrokeDashArray = new SvgUnitCollection { new SvgUnit(3), new SvgUnit(3) },
                        Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                        Children =
                        {
                            new SvgRectangle()
                            {
                                X = 100,
                                Y = 150,
                                Width = 300,
                                Height = 50,
                                StrokeDashArray = SvgUnitCollection.Inherit,
                                Fill = SvgColourServer.Inherit,
                                Stroke = SvgColourServer.Inherit
                            }
                        }
                    }
                }
            };
            SvgDocument doc2 = null;

            // Act
            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms);
            }

            // Assert
            Assert.IsNotNull(doc2);
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            Assert.AreEqual("#ff0000", g.Fill.ToString());
            Assert.AreEqual("3 3", g.StrokeDashArray.ToString());
            Assert.AreEqual("#00ff00", g.Stroke.ToString());

            var r = g.Children.OfType<SvgRectangle>().Single();
            Assert.AreEqual(100, r.X.Value);
            Assert.AreEqual(150, r.Y.Value);
            Assert.AreEqual(300, r.Width.Value);
            Assert.AreEqual(50, r.Height.Value);
            Assert.AreEqual("#ff0000", r.Fill.ToString());
            Assert.AreEqual("3 3", r.StrokeDashArray.ToString());
            Assert.AreEqual("#00ff00", r.Stroke.ToString());
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }

        [Test]
        public void SavingDocument_KeepsUnserAttributesIntact()
        {
            // Arrange
            var doc = new SvgDocument()
            {
                Children =
                {
                    new SvgGroup()
                    {
                        Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                        StrokeDashArray = new SvgUnitCollection { new SvgUnit(3), new SvgUnit(3) },
                        Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                        Children =
                        {
                            new SvgRectangle()
                            {
                                X = 100,
                                Y = 150,
                                Width = 300,
                                Height = 50,
                                StrokeDashArray = null,
                                Fill = null,
                                Stroke = null
                            }
                        }
                    }
                }
            };
            SvgDocument doc2 = null;

            // Act
            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms);
            }

            // Assert
            Assert.IsNotNull(doc2);
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            Assert.AreEqual("#ff0000", g.Fill.ToString());
            Assert.AreEqual("3 3", g.StrokeDashArray.ToString());
            Assert.AreEqual("#00ff00", g.Stroke.ToString());

            var r = g.Children.OfType<SvgRectangle>().Single();
            Assert.AreEqual(100, r.X.Value);
            Assert.AreEqual(150, r.Y.Value);
            Assert.AreEqual(300, r.Width.Value);
            Assert.AreEqual(50, r.Height.Value);
            Assert.AreEqual("#ff0000", r.Fill.ToString());
            Assert.AreEqual("3 3", r.StrokeDashArray.ToString());
            Assert.AreEqual("#00ff00", r.Stroke.ToString());
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }


        [Test]
        public void SavingDocument_KeepsNoneIfNoneIsSetExplicitly()
        {
            // Arrange
            var doc = new SvgDocument()
            {
                Children =
                {
                    new SvgGroup()
                    {
                        Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                        Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                        Children =
                        {
                            new SvgRectangle()
                            {
                                X = 100,
                                Y = 150,
                                Width = 300,
                                Height = 50,
                                Fill = SvgPaintServer.None,
                                Stroke = SvgPaintServer.None
                            }
                        }
                    }
                }
            };
            SvgDocument doc2 = null;

            // Act
            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms);
            }

            // Assert
            Assert.IsNotNull(doc2);
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            Assert.AreEqual("#ff0000", g.Fill.ToString());
            Assert.AreEqual("#00ff00", g.Stroke.ToString());

            var r = g.Children.OfType<SvgRectangle>().Single();
            Assert.AreEqual(100, r.X.Value);
            Assert.AreEqual(150, r.Y.Value);
            Assert.AreEqual(300, r.Width.Value);
            Assert.AreEqual(50, r.Height.Value);
            Assert.AreSame(SvgPaintServer.None, r.Fill);
            Assert.AreSame(SvgPaintServer.None, r.Stroke);
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }

        /*
         * style="fill:none;fill-opacity:0;stroke:none"
         */

        private static void AssertInheritedAttribute(SvgRectangle r, string attributeName)
        {
            string val;
            if (r.TryGetAttribute("attributeName", out val))
                Assert.AreEqual("inherit", val);
        }
    }
}
