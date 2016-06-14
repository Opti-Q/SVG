using Android.App;
using Android.OS;
using Android.Views;
using System.Linq;
using MvvmCross.Droid.Views;
using Svg.Droid.Editor;
using Svg.Droid.SampleEditor.Core.ViewModels;

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
            this.ViewModel.Canvas = _padView.DrawingCanvas;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            foreach (var commands in ViewModel.Canvas.ToolCommands)
            {
                var cmds = commands.ToArray();
                if (cmds.Length == 0)
                    continue;

                if (cmds.Count() == 1)
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
