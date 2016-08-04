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

        public Task<int> GetIndexFromUserInput(string title, string[] items)
        {
            var builder = new AlertDialog.Builder(Context);
            var tcs = new TaskCompletionSource<int>();
            builder.SetTitle(title);
            builder.SetItems(items, (sender, args) =>
            {
                tcs.SetResult(args.Which);
            });
            builder.Show();

            return tcs.Task;
        }


    }
}