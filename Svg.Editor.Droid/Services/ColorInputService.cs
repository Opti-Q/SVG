using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Java.Lang;
using Svg.Core.Tools;
using Svg.Interfaces;

namespace Svg.Droid.Editor.Services
{
    public class ColorInputService : IColorInputService
    {
        private Context Context { get; }

        public ColorInputService(Context context)
        {
            Context = context;
        }

        public Task<Color> GetUserInput(string title, Color[] items)
        {
            var builder = new AlertDialog.Builder(Context);
            var tcs = new TaskCompletionSource<Color>();
            builder.SetTitle(title);
            builder.SetItems(items.Select(x => x.IsNamedColor ? x.Name : $"{x.R}, {x.G}, {x.B}").ToArray(), (sender, args) =>
            {
                var selectedColor = items[args.Which];
                tcs.SetResult(selectedColor);
            });
            builder.Show();

            return tcs.Task;
        }


    }
}