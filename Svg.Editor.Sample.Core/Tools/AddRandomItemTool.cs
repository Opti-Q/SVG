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

                    var trans = child.Transforms.OfType<SvgTranslate>().FirstOrDefault();
                    SvgTranslate tl = new SvgTranslate(-canvas.Translate.X, -canvas.Translate.Y);
                    if (trans != null)
                    {
                        child.Transforms.Remove(trans);
                        tl = new SvgTranslate(trans.X - tl.X, trans.Y - tl.Y);
                    }
                    child.Transforms.Add(tl);

                    _canvas.Document.Children.Add(child);

                    _canvas.InvalidateCanvas();
                } )
            };
        }
        public Func<string, ISvgSource> SourceProvider { get; set; }

        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
        }

        
    }
}
