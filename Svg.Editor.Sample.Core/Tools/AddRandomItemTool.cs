using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Core;
using Svg.Core.Events;
using Svg.Core.Tools;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class AddRandomItemTool : ToolBase
    {

        private readonly SvgDrawingCanvas _canvas;

        public AddRandomItemTool(SvgDrawingCanvas canvas, Func<string, ISvgSource> sourceProvider = null) : base("Add random item")
        {
            SourceProvider = sourceProvider;
            _canvas = canvas;
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Add random item", (obj) =>
                {
                    if (SourceProvider == null)
                        return;
                    var provider = SourceProvider("isolib/Straights/solid and broken/solid1.svg");
                    //var provider = SourceProvider("svg/painting-control-01-f.svg");
                    var otherDoc = SvgDocument.Open<SvgDocument>(provider);
                    var child = otherDoc.Children.OfType<SvgVisualElement>().First(e => e.Displayable && e.Visible);
                    var z = canvas.ZoomFactor;
                    var halfRelWidth = canvas.ScreenWidth/z/2;
                    var halfRelHeight = canvas.ScreenHeight/z/2;
                    var childBounds = child.Bounds;
                    var halfRelChildWidth = childBounds.Width/2;
                    var halfRelChildHeight = childBounds.Height/2;

                    SvgTranslate tl = new SvgTranslate(-canvas.RelativeTranslate.X + halfRelWidth - halfRelChildWidth, -canvas.RelativeTranslate.Y + halfRelHeight - halfRelChildHeight);
                    child.Transforms.Add(tl);

                    _canvas.Document.Children.Add(child);

                    _canvas.FireInvalidateCanvas();
                } )
            };
        }
        public Func<string, ISvgSource> SourceProvider { get; set; }
    }
}
