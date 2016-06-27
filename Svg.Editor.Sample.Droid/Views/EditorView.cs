using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using MvvmCross.Droid.Views;
using Svg.Droid.Editor;
using Svg.Droid.SampleEditor.Core;
using Svg.Droid.SampleEditor.Core.ViewModels;
using Svg.Interfaces;
using Svg.Platform;
using Path = System.IO.Path;

namespace Svg.Droid.SampleEditor.Views
{
    [Activity(Label = "Edit SVG", Exported=true)]
    public class EditorView : MvxActivity
    {
        private SvgDrawingCanvasView _padView;
        private Dictionary<string, int> _iconCache = new Dictionary<string, int>();

        protected override void OnCreate(Bundle bundle)
        {
            // register first
            SvgPlatformSetup.Init(new SvgAndroidPlatformOptions() {EnableFastTextRendering = true});
            Engine.Register<ISvgSourceFactory, SvgSourceFactory>(() => new SvgSourceFactory(Assets));

            SetupIconCache();

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EditorVIew);
            _padView = FindViewById<SvgDrawingCanvasView>(Resource.Id.pad);

            _padView.DrawingCanvas = this.ViewModel.Canvas;


            RemoveShortcut();
            AddShortcut();
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
            var shownActions = 2;

            foreach (var commands in ViewModel.Canvas.ToolCommands)
            {
                var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
                if (cmds.Length == 0)
                    continue;

                if (cmds.Length == 1)
                {
                    var cmd = cmds.Single();
                    var mi = menu.Add(cmd.GetHashCode(), cmd.GetHashCode(), 1, cmd.Name);
                    mi.SetIcon(GetIconIdFromName(cmd.IconName));

                    if (shownActions > 0)
                        mi.SetShowAsAction(ShowAsAction.IfRoom);
                    else
                        mi.SetShowAsAction(ShowAsAction.Never);
                }
                else
                {
                    var c = cmds.First();

                    var m = menu.AddSubMenu(c.Tool.GetHashCode(), c.Tool.GetHashCode(), 1, c.GroupName);
                    m.SetIcon(GetIconIdFromName(c.GroupIconName));

                    foreach (var cmd in cmds)
                    {
                        var mi = m.Add(cmd.GetHashCode(), cmd.GetHashCode(), 1, cmd.Name);
                        mi.SetIcon(GetIconIdFromName(cmd.IconName));
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

        private int GetIconIdFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return 0;

            var n = Path.GetFileNameWithoutExtension(name);

            if (string.IsNullOrWhiteSpace(n))
                return 0;

            int value;
            if (_iconCache.TryGetValue(n, out value))
                return value;

            return 0;
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

        private void RemoveShortcut()
        {
            var shortcutIntent = new Intent(this, typeof(EditorView));
            shortcutIntent.SetAction(Intent.ActionMain);

            var intent = new Intent();
            intent.PutExtra(Intent.ExtraShortcutIntent, shortcutIntent);
            intent.PutExtra(Intent.ExtraShortcutName, "My Awesome App!");
            intent.SetAction("com.android.launcher.action.UNINSTALL_SHORTCUT");
            SendBroadcast(intent);
        }

        private void AddShortcut()
        {
            var shortcutIntent = new Intent(this, typeof(EditorView));
            shortcutIntent.SetAction(Intent.ActionMain);

            //var iconResource = Intent.ShortcutIconResource.FromContext(
            //    this, Resource.Drawable.Icon);

            var folder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)
                            .AbsolutePath;
            var filename = "shortcut.png";

            var theBitmap = BitmapFactory.DecodeFile(Path.Combine(folder, filename));
            var scaledBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(theBitmap, 128, 128, true);

            var intent = new Intent();
            intent.PutExtra(Intent.ExtraShortcutIntent, shortcutIntent);
            intent.PutExtra(Intent.ExtraShortcutName, "SVG Editor");
            //intent.PutExtra(Intent.ExtraShortcutIconResource, iconResource);
            intent.PutExtra(Intent.ExtraShortcutIcon, scaledBitmap);
            intent.SetAction("com.android.launcher.action.INSTALL_SHORTCUT");
            SendBroadcast(intent);
        }
    }

    public class SvgSourceFactory : ISvgSourceFactory
    {
        private readonly AssetManager _assets;

        public SvgSourceFactory(AssetManager assets)
        {
            _assets = assets;
        }

        public ISvgSource Create(string path)
        {
            return new SvgAssetSource(path, _assets);
        }
    }
}
