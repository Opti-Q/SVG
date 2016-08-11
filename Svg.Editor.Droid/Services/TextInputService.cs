using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Views.InputMethods;
using Android.Widget;
using Svg.Core.Tools;

namespace Svg.Droid.Editor.Services
{
    public class TextInputService : ITextInputService
    {
        private readonly Context _context;

        public TextInputService(Context context)
        {
            _context = context;
        }

        public Task<string> GetUserInput(string title, string textValue)
        {
            var tcs = new TaskCompletionSource<string>();

            AlertDialog.Builder builder = new AlertDialog.Builder(_context);
            builder.SetTitle(title);

            // Set up the input
            var input = new EditText(_context)
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