using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Svg.Core.Tools;
using Svg.Interfaces;
using Object = Java.Lang.Object;

namespace Svg.Droid.Editor.Services
{
    public class ColorInputService : IColorInputService
    {
        private Context Context { get; }

        public ColorInputService(Context context)
        {
            Context = context;
        }

        public Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors)
        {
            var builder = new AlertDialog.Builder(Context);
            var tcs = new TaskCompletionSource<int>();
            builder.SetTitle(title);
            builder.SetAdapter(new ColorListAdapter(items, colors, Context), (sender, args) =>
            {
                tcs.SetResult(args.Which);
            });
            //builder.SetItems(items, (sender, args) =>
            //{
            //    tcs.SetResult(args.Which);
            //});
            builder.Show();

            return tcs.Task;
        }


    }

    internal class ColorListAdapter : BaseAdapter
    {
        protected string[] Items { get; }
        protected string[] Colors { get; }
        protected Context Context { get; }

        public ColorListAdapter(string[] items, string[] colors, Context context)
        {
            Items = items;
            Colors = colors;
            Context = context;
        }

        public override Object GetItem(int position)
        {
            return new ColorItem { Title = Items[position], Color = Colors[position] };
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var layoutInflater = Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            var view = convertView ?? layoutInflater?.Inflate(Android.Resource.Layout.ActivityListItem, parent, false);

            if (view == null) return null;

            var icon = view.FindViewById<ImageView>(Android.Resource.Id.Icon);
            var text = view.FindViewById<TextView>(Android.Resource.Id.Text1);

            //var svgCachingService = Engine.Resolve<SvgC>()
            //icon.Drawable = Drawable.CreateFromPath();
            text.Text = Items[position];

            return view;
        }

        public override int Count => Items.Length;

        protected class ColorItem : Object
        {
            public string Title { get; set; }
            public string Color { get; set; }
        }
    }
}