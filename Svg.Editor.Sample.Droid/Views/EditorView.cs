using Android.App;
using Android.OS;
using Android.Views;
using System.Linq;
using MvvmCross.Droid.Views;
using Svg.Droid.Editor;
using Svg.Droid.SampleEditor.Core.Tools;
using Svg.Droid.SampleEditor.Core.ViewModels;
using Svg.Platform;

namespace Svg.Droid.SampleEditor.Views
{
    [Activity(Label = "Edit SVG")]
    public class EditorView : MvxActivity
    {
        private SvgDrawingCanvasView _padView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EditorVIew);
            _padView = FindViewById<SvgDrawingCanvasView>(Resource.Id.pad);
            
            // modify tool so that 
            var tool = _padView.DrawingCanvas.Tools.OfType<AddRandomItemTool>().FirstOrDefault();
            if (tool == null)
            {
                tool = new AddRandomItemTool(_padView.DrawingCanvas);
                _padView.DrawingCanvas.Tools.Add(tool);    
            }
            // this surely creates a memory leak!!
            tool.SourceProvider = (str) => new SvgAssetSource(str, this.Assets);


            _padView.DrawingCanvas.Document = SvgDocument.Open<SvgDocument>(new SvgAssetSource("isolib/Straights/solid and broken/solid1.svg", Assets));
            //_padView.DrawingCanvas.Document = SvgDocument.Open<SvgDocument>(new SvgAssetSource("svg/painting-control-01-f.svg", Assets));
            //_padView.DrawingCanvas.Document = SvgDocument.Open<SvgDocument>(new SvgAssetSource("svg/ellipse.svg", Assets));
            //_padView.DrawingCanvas.Document = SvgDocument.Open<SvgDocument>(new SvgAssetSource("svg/coords-trans-09-t.svg", Assets));
           

            // set canvas in viewmodel
            this.ViewModel.Canvas = _padView.DrawingCanvas;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            foreach (var commands in ViewModel.Canvas.ToolCommands)
            {
                var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
                if (cmds.Length == 0)
                    continue;

                if (cmds.Length == 1)
                {
                    var cmd = cmds.Single();
                    menu.Add(cmd.GetHashCode(), cmd.GetHashCode(), 1, cmd.Name);
                }
                else
                {
                    var c = cmds.First();

                    var m = menu.AddSubMenu(c.Tool.GetHashCode(), c.Tool.GetHashCode(), 1, c.Tool.Name);
                    foreach (var cmd in cmds)
                    {
                        m.Add(cmd.GetHashCode(), cmd.GetHashCode(), 1, cmd.Name);
                    }

                }
            }

            return true;
        }

        public new EditorViewModel ViewModel
        {
            get { return (EditorViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var cmd = ViewModel.Canvas.ToolCommands.SelectMany(c => c).FirstOrDefault(c => c.GetHashCode() == item.ItemId);
            if (cmd != null)
            {
                cmd.Execute(_padView);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
