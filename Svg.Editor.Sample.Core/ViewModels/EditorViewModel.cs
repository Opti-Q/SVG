using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using Svg.Core;
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
            
            Canvas.Tools.Add(new SaveTool(false));
            Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("svg/large_svg_01.svg"));
            Canvas.IsDebugEnabled = true;

            //// only allow to rotate text elements
            var rt = Canvas.Tools.OfType<RotationTool>().Single();
            //rt.Filter = (ve => ve is SvgTextBase);
            rt.RotationStep = 30; // rotate in 30 degree steps
        }

        private ISvgSource GetSource(string source)
        {
            return Engine.Resolve<ISvgSourceFactory>().Create(source);
        }
    }
}
