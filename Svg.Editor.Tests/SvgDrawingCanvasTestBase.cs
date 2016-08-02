using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Svg.Core;

namespace Svg.Editor.Core.Tests
{
    public abstract class SvgDrawingCanvasTestBase
    {
        private SvgDrawingCanvas _canvas;

        protected SvgDrawingCanvas Canvas => _canvas;

        static SvgDrawingCanvasTestBase()
        {
            SvgPlatformSetup.Init(new SvgSkiaPlatformOptions() {EnableFastTextRendering = true});
        }

        [SetUp]
        public virtual void SetUp()
        {
            _canvas = new SvgDrawingCanvas();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _canvas.Dispose();
        }
    }
}
