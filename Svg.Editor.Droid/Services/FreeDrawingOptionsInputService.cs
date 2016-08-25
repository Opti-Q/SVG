using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Android.Widget;
using Svg;
using Svg.Core.Tools;
using Svg.Droid.Editor.Services;

[assembly: SvgService(typeof(IFreeDrawingOptionsInputService), typeof(FreeDrawingOptionsInputService))]
namespace Svg.Droid.Editor.Services
{
    public class FreeDrawingOptionsInputService : IFreeDrawingOptionsInputService
    {
        public Task<int[]> GetUserInput(string title, IEnumerable<string> lineStyleOptions, int lineStyleSelected, IEnumerable<string> strokeWidthOptions, int strokeWidthSelected)
        {
            var cp = Engine.Resolve<IContextProvider>();
            var context = cp.Context;

            var builder = new AlertDialog.Builder(context);
            var tcs = new TaskCompletionSource<int[]>();
            var result = new int[2];

            // setup builder
            builder.SetTitle(title);

            // setup spinner for start marker
            var view = new LinearLayout(context) { Orientation = Orientation.Horizontal };

            // setup spinner for stroke width
            var spinner1Layout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            var spinner1Label = new TextView(context) { Text = "Stoke width" };
            spinner1Layout.AddView(spinner1Label);
            var spinner1 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem, strokeWidthOptions.ToArray())
            };
            spinner1.ItemSelected += (sender, args) => result[0] = args.Position;
            spinner1.SetSelection(strokeWidthSelected);
            spinner1Layout.AddView(spinner1);
            view.AddView(spinner1Layout);

            // setup spinner for dash style
            var spinner2Layout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            var spinner2Label = new TextView(context) { Text = "Line style" };
            spinner2Layout.AddView(spinner2Label);
            var spinner2 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem, lineStyleOptions.ToArray())
            };
            spinner2.ItemSelected += (sender, args) => result[1] = args.Position;
            spinner2.SetSelection(lineStyleSelected);
            spinner2Layout.AddView(spinner2);
            view.AddView(spinner2Layout);

            var okButton = new Button(context) { Text = "OK" };
            view.AddView(okButton);

            builder.SetView(view);

            // show dialog
            var dialog = builder.Show();

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