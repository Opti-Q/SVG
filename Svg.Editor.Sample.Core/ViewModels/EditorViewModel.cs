using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using Svg.Core;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
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
            Canvas.Tools.Add(new AuxiliaryLineTool()); // auxiliar line tool
            
            Canvas.Tools.Add(new SaveTool(false));
            Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("svg/large_svg_01.svg"));
            //Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("svg/ground_floor_plan.svg"));
            Canvas.IsDebugEnabled = true;
        }

        private ISvgSource GetSource(string source)
        {
            return Engine.Resolve<ISvgSourceFactory>().Create(source);
        }
    }
}
