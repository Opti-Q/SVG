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
            
            Canvas.Tools.Add(new SaveTool(true));
            //Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("svg/ellipse.svg"));
            
        }

        private ISvgSource GetSource(string source)
        {
            return Engine.Resolve<ISvgSourceFactory>().Create(source);
        }
    }
}
