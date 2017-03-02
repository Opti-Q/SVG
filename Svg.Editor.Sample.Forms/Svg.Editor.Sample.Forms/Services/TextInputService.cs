using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Svg.Editor.Tools;

namespace Svg.Editor.Sample.Forms.Services
{
    public class TextInputService : ITextInputService
    {
        public async Task<TextTool.TextProperties> GetUserInput(string title, string textValue, IEnumerable<string> textSizeOptions, int textSizeSelected)
        {
            var result = await UserDialogs.Instance.PromptAsync(textValue, title, null, null, "write text here", InputType.Default);

            return new TextTool.TextProperties {FontSizeIndex = textSizeOptions.Count()-1, LineHeight = 12f, Text = result.Text};
        }
    }
}
