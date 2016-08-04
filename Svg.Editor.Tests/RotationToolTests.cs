using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class RotationToolTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public async Task NoElementSelected_Rotate_HasNoEffect()
        {
            // Arrange

            // Act

            // Assert
        }


        [Test]
        public async Task MultipleElementsSelected_Rotate_HasNoEffect()
        {

        }


        [Test]
        public async Task SingleElementSelected_Rotate_RotatesElementUsingSvgMatrix()
        {

        }
    }
}
