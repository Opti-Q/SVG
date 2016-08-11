using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Svg.Core.Tools;

namespace Svg.Droid.Editor.Services
{
    public class LineOptionsInputService : ILineOptionsInputService
    {
        private Context Context { get; }

        public LineOptionsInputService(Context context)
        {
            Context = context;
        }

        public Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> lineStyleOptions, int dashSelected, IEnumerable<string> markerEndOptions, int markerEndSelected)
        {
            var builder = new AlertDialog.Builder(Context);
            var tcs = new TaskCompletionSource<int[]>();
            var result = new int[3];
            AlertDialog dialog = null;

            // setup builder
            builder.SetTitle(title);

            // setup spinner for start marker
            var view = new LinearLayout(Context) { Orientation = Orientation.Horizontal };
            var spinner1 = new Spinner(Context)
            {
                Adapter =
                    new ArrayAdapter(Context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                        markerStartOptions.ToArray()),
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            spinner1.ItemSelected += (sender, args) => result[0] = args.Position;
            spinner1.SetSelection(markerStartSelected);
            view.AddView(spinner1);

            // setup spinner for dash style
            var spinner2 = new Spinner(Context)
            {
                Adapter =
                    new ArrayAdapter(Context, Android.Resource.Layout.SimpleSpinnerDropDownItem, lineStyleOptions.ToArray()),
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            spinner2.ItemSelected += (sender, args) => result[1] = args.Position;
            spinner2.SetSelection(dashSelected);
            view.AddView(spinner2);

            // setup spinner for end marker
            var spinner3 = new Spinner(Context)
            {
                Adapter =
                    new ArrayAdapter(Context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                        markerEndOptions.ToArray()),
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            spinner3.ItemSelected += (sender, args) => result[2] = args.Position;
            spinner3.SetSelection(markerEndSelected);
            view.AddView(spinner3);

            var okButton = new Button(Context) { Text = "OK" };
            view.AddView(okButton);

            builder.SetView(view);

            // show dialog
            dialog = builder.Show();

            okButton.Click += (sender, args) =>
            {
                tcs.TrySetResult(result);
                dialog?.Dismiss();
            };

            return tcs.Task;
        }
    }

    //internal class ColorListAdapter : BaseAdapter
    //{
    //    protected string[] Items { get; }
    //    protected string[] Colors { get; }
    //    protected Context Context { get; }

    //    public ColorListAdapter(string[] items, string[] colors, Context context)
    //    {
    //        Items = items;
    //        Colors = colors;
    //        Context = context;
    //    }

    //    public override Java.Lang.Object GetItem(int position)
    //    {
    //        return new ColorItem { Title = Items[position], Color = Colors[position] };
    //    }

    //    public override long GetItemId(int position)
    //    {
    //        return position;
    //    }

    //    public override View GetView(int position, View convertView, ViewGroup parent)
    //    {
    //        var layoutInflater = Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
    //        var view = convertView ?? layoutInflater?.Inflate(Android.Resource.Layout.SimpleListItem1, parent, false);

    //        if (view == null) return null;

    //        var text = view.FindViewById<TextView>(Android.Resource.Id.Text1);

    //        text.Text = Items[position];

    //        var bgColor = Android.Graphics.Color.ParseColor(Colors[position]);
    //        view.SetBackgroundColor(bgColor);

    //        var brightness = bgColor.R * .299 + bgColor.G * .587 + bgColor.B * .114;
    //        text.SetTextColor(brightness > 128 ? Android.Graphics.Color.Black : Android.Graphics.Color.White);

    //        return view;
    //    }

    //    public override int Count => Items.Length;

    //    protected class ColorItem : Java.Lang.Object
    //    {
    //        public string Title { get; set; }
    //        public string Color { get; set; }
    //    }
    //}
}