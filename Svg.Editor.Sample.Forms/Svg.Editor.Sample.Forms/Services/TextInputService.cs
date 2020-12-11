using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Sample.Forms.Services
{
    public class TextInputService : ITextInputService
    {
        public async Task<TextTool.TextProperties> GetUserInput(string title, string textValue, IEnumerable<string> textSizeOptions, int textSizeSelected)
        {
            var result = await UserDialogs.Instance.PromptAsync("Text edit", title, "Ok", "Cancel", textValue ?? "Enter text...", InputType.Default);
            var defaultResult = new TextTool.TextProperties
                {
                    FontSizeIndex = textSizeSelected,
                    LineHeight = 12f,
                    Text = textValue
                };
            ;
            var text = result.Text;
            if (text == "Cancel")
            {
                return defaultResult;
            }

            var sizeResult = await UserDialogs.Instance.ActionSheetAsync("Font size", "Cancel", null, null, textSizeOptions.ToArray());
            
            if(sizeResult == "Cancel")
            {
                return defaultResult;
            }

            var sizeIndex = textSizeOptions.ToList().IndexOf(sizeResult);
            sizeIndex = sizeIndex >= 0 ? sizeIndex : textSizeSelected;

            return new TextTool.TextProperties {FontSizeIndex = sizeIndex, LineHeight = 12f, Text = text};
        }

        public async Task<string> GetUserInput(string textValue = null)
        {
            var defaultResult = "";

            PromptResult result;
            do
            {
                result = await UserDialogs.Instance.PromptAsync("Text edit", "Please enter 1 or 2 characters.", "Ok", "Cancel", textValue ?? "Enter text...");
            }
            while (result.Text.Length > 2);

            var text = result.Text;
            if (text == "Cancel")
            {
                return defaultResult;
            }

            return text;
        }
    }
}
