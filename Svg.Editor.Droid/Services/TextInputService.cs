using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Views.InputMethods;
using Android.Widget;
using Svg;
using Svg.Core.Tools;
using Svg.Droid.Editor.Services;

[assembly: SvgService(typeof(ITextInputService), typeof(TextInputService))]
namespace Svg.Droid.Editor.Services
{
    public class TextInputService : ITextInputService
    {

        public Task<string> GetUserInput(string title, string textValue)
        {
            var tcs = new TaskCompletionSource<string>();

            var cp = Engine.Resolve<IContextProvider>();
            var context = cp.Context;

            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);

            // Set up the input
            var input = new EditText(context)
            {
                Text = textValue,
                InputType = InputTypes.TextFlagMultiLine,
                ImeOptions = ImeAction.None
            };
            input.SetSingleLine(false);
            builder.SetView(input);

            builder.SetPositiveButton("OK", (sender, args) =>
            {
                tcs.TrySetResult(input.Text);
            });

            builder.SetNegativeButton("Cancel", (sender, args) =>
            {
                tcs.TrySetResult(textValue);
            });

            builder.SetCancelable(false);
            builder.Show();

            return tcs.Task;
        }
    }
}