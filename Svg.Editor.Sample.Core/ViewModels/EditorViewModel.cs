using System;
using MvvmCross.Core.ViewModels;
using Svg.Core;

namespace Svg.Droid.SampleEditor.Core.ViewModels
{
    public class EditorViewModel 
        : MvxViewModel
    {        
        public SvgDrawingCanvas Canvas { get; set; }
    }
}
