using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Android.Widget;
using Svg;
using Svg.Core.Tools;
using Svg.Droid.Editor.Services;

[assembly: SvgService(typeof(ILineOptionsInputService), typeof(LineOptionsInputService))]
namespace Svg.Droid.Editor.Services
{
    public class LineOptionsInputService : ILineOptionsInputService
    {
        public Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> lineStyleOptions, int dashSelected, IEnumerable<string> markerEndOptions, int markerEndSelected)
        {
            var cp = Engine.Resolve<IContextProvider>();
            var context = cp.Context;

            var builder = new AlertDialog.Builder(context);
            var tcs = new TaskCompletionSource<int[]>();
            var result = new int[3];

            // setup builder
            builder.SetTitle(title);

            // setup spinner for start marker
            var view = new LinearLayout(context) { Orientation = Orientation.Horizontal };

            view.SetPadding(40, 12, 40, 0);

            var spinner1 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                        markerStartOptions.ToArray()),
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            spinner1.ItemSelected += (sender, args) => result[0] = args.Position;
            spinner1.SetSelection(markerStartSelected);
            view.AddView(spinner1);

            // setup spinner for dash style
            var spinner2 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem, lineStyleOptions.ToArray()),
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            spinner2.ItemSelected += (sender, args) => result[1] = args.Position;
            spinner2.SetSelection(dashSelected);
            view.AddView(spinner2);

            // setup spinner for end marker
            var spinner3 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                        markerEndOptions.ToArray()),
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            spinner3.ItemSelected += (sender, args) => result[2] = args.Position;
            spinner3.SetSelection(markerEndSelected);
            view.AddView(spinner3);

            builder.SetView(view);

            builder.SetPositiveButton("OK", (sender, args) => tcs.TrySetResult(result));

            // show dialog
            builder.Show();

            return tcs.Task;
        }
    }
}