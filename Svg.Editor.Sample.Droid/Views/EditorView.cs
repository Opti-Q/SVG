using System;
using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.OS;
using Android.Views;
using System.Linq;
using Android.Graphics.Drawables;
using MvvmCross.Droid.Views;
using Svg.Core.Interfaces;
using Svg.Core.Tools;
using Svg.Droid.Editor;
using Svg.Droid.Editor.Services;
using Svg.Droid.SampleEditor.Core;
using Svg.Droid.SampleEditor.Core.ViewModels;
using Svg.Interfaces;
using Path = System.IO.Path;

namespace Svg.Droid.SampleEditor.Views
{
    [Activity(Label = "Edit SVG", Exported = true)]
    public class EditorView : MvxActivity
    {
        private SvgDrawingCanvasView _padView;
        private Dictionary<string, int> _iconCache = new Dictionary<string, int>();

        protected override void OnCreate(Bundle bundle)
        {
            // register first
            // Initialize SVG Platform and tie together PCL and platform specific modules
            SvgEditor.Init(this);

            SetupIconCache();

            base.OnCreate(bundle);

            SetContentView(Resource.Layout.EditorView);
            _padView = FindViewById<SvgDrawingCanvasView>(Resource.Id.pad);

            _padView.DrawingCanvas = ViewModel.Canvas;

        }

        private void SetupIconCache()
        {
            var t = typeof(Resource.Drawable);
            foreach (var constant in t.GetFields().Where(@fld => @fld.IsLiteral))
            {
                var rawValue = constant.GetRawConstantValue();
                if (!(rawValue is int))
                    continue;

                _iconCache.Add(constant.Name, (int)rawValue);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var shownActions = 3;

            foreach (var commands in ViewModel.Canvas.ToolCommands)
            {
                var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
                if (cmds.Length == 0)
                    continue;

                if (cmds.Length == 1)
                {
                    var cmd = cmds.Single();
                    var mi = menu.Add(cmd.GetHashCode(), cmd.GetHashCode(), 1, cmd.Name);
                    SetIcon(mi, cmd.IconName);

                    if (shownActions > 0)
                        mi.SetShowAsAction(ShowAsAction.IfRoom);
                    else
                        mi.SetShowAsAction(ShowAsAction.Never);
                }
                else
                {
                    var c = cmds.First();

                    var m = menu.AddSubMenu(c.Tool.GetHashCode(), c.Tool.GetHashCode(), 1, c.GroupName);
                    SetIcon(m.Item, c.GroupIconName);

                    foreach (var cmd in cmds)
                    {
                        var mi = m.Add(cmd.GetHashCode(), cmd.GetHashCode(), 1, cmd.Name);
                        SetIcon(mi, cmd.IconName);
                    }

                    if (shownActions > 0)
                        m.Item.SetShowAsAction(ShowAsAction.IfRoom);
                    else
                        m.Item.SetShowAsAction(ShowAsAction.Never);

                }

                shownActions--;
            }

            return true;
        }

        private void SetIcon(IMenuItem item, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var n = Path.GetFileNameWithoutExtension(name);

            if (string.IsNullOrWhiteSpace(n))
                return;

            int value;
            if (_iconCache.TryGetValue(n, out value))
            {
                item.SetIcon(value);
            }
            else if(File.Exists(name))
            {
                item.SetIcon(Drawable.CreateFromPath(name));
            }
        }

        public new EditorViewModel ViewModel
        {
            get { return (EditorViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var cmd =
                ViewModel.Canvas.ToolCommands.SelectMany(c => c).FirstOrDefault(c => c.GetHashCode() == item.ItemId);
            if (cmd != null)
            {
                cmd.Execute(_padView);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
