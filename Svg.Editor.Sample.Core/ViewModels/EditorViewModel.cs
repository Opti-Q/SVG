using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using Svg.Core;
using Svg.Droid.SampleEditor.Core.Tools;
using Svg.Interfaces;

namespace Svg.Droid.SampleEditor.Core.ViewModels
{
    public class EditorViewModel 
        : MvxViewModel
    {
        public SvgDrawingCanvas Canvas { get; set; } = new SvgDrawingCanvas();

        public void Init()
        {
            // modify tool so that 
            var tool = Canvas.Tools.OfType<AddRandomItemTool>().FirstOrDefault();
            if (tool == null)
            {
                tool = new AddRandomItemTool(Canvas);
                Canvas.Tools.Add(tool);
            }
            // this surely creates a memory leak!!
            tool.SourceProvider = GetSource;
            
            Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("isolib/Straights/solid and broken/solid1.svg"));
            //_padView.DrawingCanvas.Document = SvgDocument.Open<SvgDocument>(new SvgAssetSource("svg/painting-control-01-f.svg", Assets));
            //_padView.DrawingCanvas.Document = SvgDocument.Open<SvgDocument>(new SvgAssetSource("svg/ellipse.svg", Assets));
            //_padView.DrawingCanvas.Document = SvgDocument.Open<SvgDocument>(new SvgAssetSource("svg/coords-trans-09-t.svg", Assets));
            
        }

        private ISvgSource GetSource(string source)
        {
            return Engine.Resolve<ISvgSourceFactory>().Create(source);
        }
    }
}
