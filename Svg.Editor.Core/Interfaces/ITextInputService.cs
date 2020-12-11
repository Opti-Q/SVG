using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Editor.Tools;

namespace Svg.Editor.Interfaces
{
    public interface ITextInputService
    {
        Task<TextTool.TextProperties> GetUserInput(
            string title,
            string textValue,
            IEnumerable<string> textSizeOptions,
            int textSizeSelected);

        Task<string> GetUserInput(
            string textValue = null);
    }
}